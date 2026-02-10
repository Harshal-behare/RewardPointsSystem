using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Users;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Users
{
    /// <summary>
    /// Service: UserQueryService
    /// Responsibility: Query user data with related entities (read operations)
    /// Clean Architecture Compliant - Encapsulates data access logic
    /// </summary>
    public class UserQueryService : IUserQueryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserQueryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersWithDetailsAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
            var roles = await _unitOfWork.Roles.GetAllAsync();
            var pointsAccounts = await _unitOfWork.UserPointsAccounts.GetAllAsync();

            return users.Select(user => MapToResponseDto(user, userRoles, roles, pointsAccounts));
        }

        public async Task<UserResponseDto?> GetUserWithDetailsAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return null;

            var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId);
            var roles = await _unitOfWork.Roles.GetAllAsync();
            var pointsAccount = await _unitOfWork.UserPointsAccounts.SingleOrDefaultAsync(pa => pa.UserId == userId);

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var roleNames = roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();

            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roleNames,
                PointsBalance = pointsAccount?.CurrentBalance
            };
        }

        public async Task<IEnumerable<UserResponseDto>> GetActiveUsersWithDetailsAsync()
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.IsActive);
            var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
            var roles = await _unitOfWork.Roles.GetAllAsync();
            var pointsAccounts = await _unitOfWork.UserPointsAccounts.GetAllAsync();

            return users.Select(user => MapToResponseDto(user, userRoles, roles, pointsAccounts));
        }

        private static UserResponseDto MapToResponseDto(
            Domain.Entities.Core.User user,
            IEnumerable<Domain.Entities.Core.UserRole> userRoles,
            IEnumerable<Domain.Entities.Core.Role> roles,
            IEnumerable<Domain.Entities.Accounts.UserPointsAccount> pointsAccounts)
        {
            var userRoleIds = userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
            var roleNames = roles.Where(r => userRoleIds.Contains(r.Id)).Select(r => r.Name).ToList();
            var pointsAccount = pointsAccounts.FirstOrDefault(pa => pa.UserId == user.Id);

            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roleNames,
                PointsBalance = pointsAccount?.CurrentBalance
            };
        }
    }
}
