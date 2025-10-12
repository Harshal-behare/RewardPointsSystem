using System;

namespace RewardPointsSystem.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a role is not found.
    /// </summary>
    public class RoleNotFoundException : DomainException
    {
        public Guid RoleId { get; }
        public string RoleName { get; }

        public RoleNotFoundException(Guid roleId) 
            : base($"Role with ID '{roleId}' was not found.")
        {
            RoleId = roleId;
        }

        public RoleNotFoundException(string roleName) 
            : base($"Role '{roleName}' was not found.")
        {
            RoleName = roleName;
        }
    }

    /// <summary>
    /// Exception thrown when attempting to create a duplicate role.
    /// </summary>
    public class DuplicateRoleException : DomainException
    {
        public string RoleName { get; }

        public DuplicateRoleException(string roleName) 
            : base($"Role '{roleName}' already exists.")
        {
            RoleName = roleName;
        }
    }

    /// <summary>
    /// Exception thrown when user role assignment is invalid.
    /// </summary>
    public class InvalidRoleAssignmentException : DomainException
    {
        public InvalidRoleAssignmentException(string message) : base(message)
        {
        }
    }
}
