using System;
using System.Collections.Generic;
using System.Linq;

namespace RewardPointsSystem.Models
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string EmployeeId { get; private set; }
        public int PointsBalance { get; private set; }
        public List<Guid> RoleIds { get; private set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; private set; }
        public Dictionary<string, object> Metadata { get; private set; }

        public User(string name, string email, string employeeId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("Employee ID is required", nameof(employeeId));

            Name = name;
            Email = email.ToLowerInvariant();
            EmployeeId = employeeId;
            PointsBalance = 0;
            RoleIds = new List<Guid>();
            Metadata = new Dictionary<string, object>();
        }

        public void AddPoints(int points)
        {
            if (points <= 0) 
                throw new ArgumentException("Points must be positive", nameof(points));
            
            PointsBalance += points;
        }

        public void DeductPoints(int points)
        {
            if (points <= 0) 
                throw new ArgumentException("Points must be positive", nameof(points));
            if (points > PointsBalance) 
                throw new InvalidOperationException($"Insufficient balance. Current balance: {PointsBalance}, Requested: {points}");
            
            PointsBalance -= points;
        }

        public void AssignRole(Guid roleId)
        {
            if (!RoleIds.Contains(roleId))
                RoleIds.Add(roleId);
        }

        public void RemoveRole(Guid roleId)
        {
            RoleIds.Remove(roleId);
        }

        public bool HasRole(Guid roleId)
        {
            return RoleIds.Contains(roleId);
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        public void AddMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Metadata key is required", nameof(key));

            Metadata[key] = value;
        }

        public T GetMetadata<T>(string key, T defaultValue = default(T))
        {
            if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            
            return defaultValue;
        }
    }
}

