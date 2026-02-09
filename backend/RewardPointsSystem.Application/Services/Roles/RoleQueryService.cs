using RewardPointsSystem.Application.DTOs.Roles;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Application.Services.Roles
{
    /// <summary>
    /// Query service implementation for role operations with DTO mapping.
    /// Centralizes all entity-to-DTO mapping in Application layer per Clean Architecture.
    /// </summary>
    public class RoleQueryService : IRoleQueryService
    {
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;

        public RoleQueryService(IRoleService roleService, IUserRoleService userRoleService)
        {
            _roleService = roleService;
            _userRoleService = userRoleService;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return roles.Select(MapToDto);
        }

        /// <inheritdoc />
        public async Task<RoleResponseDto?> GetRoleByIdAsync(Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return role == null ? null : MapToDto(role);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RoleResponseDto>> GetUserRolesAsync(Guid userId)
        {
            var roles = await _userRoleService.GetUserRolesAsync(userId);
            return roles.Select(MapToDto);
        }

        private static RoleResponseDto MapToDto(Role role)
        {
            return new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            };
        }
    }
}
