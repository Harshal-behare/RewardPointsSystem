using System;
using System.Collections.Generic;
using System.Linq;
using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleService _roleService;

        public UserService(IUnitOfWork unitOfWork, IRoleService roleService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Check for duplicate email or employee ID
            if (_unitOfWork.Users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"User with email '{user.Email}' already exists");

            if (_unitOfWork.Users.Any(u => u.EmployeeId == user.EmployeeId))
                throw new InvalidOperationException($"User with employee ID '{user.EmployeeId}' already exists");

            // Assign default Employee role to new users
            var employeeRole = _roleService.GetRoleByName("Employee");
            if (employeeRole != null)
            {
                user.AssignRole(employeeRole.Id);
            }

            _unitOfWork.Users.Add(user);
            _unitOfWork.Complete();
        }

        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return _unitOfWork.Users.SingleOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public User GetUserById(Guid id)
        {
            return _unitOfWork.Users.GetById(id);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _unitOfWork.Users.Find(u => u.IsActive);
        }

        public void UpdateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _unitOfWork.Users.Update(user);
            _unitOfWork.Complete();
        }

        public void DeactivateUser(Guid userId)
        {
            var user = GetUserById(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found");

            user.IsActive = false;
            _unitOfWork.Users.Update(user);
            _unitOfWork.Complete();
        }

        public void AssignRoleToUser(Guid userId, Guid roleId)
        {
            var user = GetUserById(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found");

            var role = _roleService.GetRoleById(roleId);
            if (role == null)
                throw new InvalidOperationException($"Role with ID {roleId} not found");

            user.AssignRole(roleId);
            _unitOfWork.Users.Update(user);
            _unitOfWork.Complete();
        }

        public void RemoveRoleFromUser(Guid userId, Guid roleId)
        {
            var user = GetUserById(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found");

            user.RemoveRole(roleId);
            _unitOfWork.Users.Update(user);
            _unitOfWork.Complete();
        }

        public bool UserHasPermission(Guid userId, string permission)
        {
            var user = GetUserById(userId);
            return user != null && _roleService.UserHasPermission(user, permission);
        }
    }
}

