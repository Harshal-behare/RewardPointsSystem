using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardPointsSystem.Models
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
        public string EmployeeId { get; set; }
        public int PointsBalance { get; private set; }

        public User(string name, string email, string employeeId)
        {
            Name = name;
            Email = email;
            EmployeeId = employeeId;
            PointsBalance = 0;
        }

        public void AddPoints(int points)
        {
            if (points <= 0) throw new ArgumentException("Points must be positive");
            PointsBalance += points;
        }

        public void DeductPoints(int points)
        {
            if (points <= 0) throw new ArgumentException("Points must be positive");
            if (points > PointsBalance) throw new InvalidOperationException("Insufficient balance");
            PointsBalance -= points;
        }
    }
}

