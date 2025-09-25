using System;
using System.Collections.Generic;
using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IUserService
    {
        void AddUser(User user);
        User GetUserByEmail(string email);
        User GetUserById(Guid id);
        IEnumerable<User> GetAllUsers();
        void UpdateUser(User user);
        void DeactivateUser(Guid userId);
        void AssignRoleToUser(Guid userId, Guid roleId);
        void RemoveRoleFromUser(Guid userId, Guid roleId);
        bool UserHasPermission(Guid userId, string permission);
    }
}


