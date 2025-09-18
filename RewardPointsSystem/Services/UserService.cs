using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users = new();

        public void AddUser(User user)
        {
            if (_users.Any(u => u.Email == user.Email || u.EmployeeId == user.EmployeeId))
                throw new InvalidOperationException("Duplicate user not allowed");

            _users.Add(user);
        }

        public User GetUserByEmail(string email) => _users.FirstOrDefault(u => u.Email == email);

        public IEnumerable<User> GetAllUsers() => _users;
    }
}

