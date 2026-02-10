using System;

namespace RewardPointsSystem.Application.DTOs.Roles
{
    /// <summary>
    /// DTO for creating a new role
    /// </summary>
    public class CreateRoleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO for role response
    /// </summary>
    public class RoleResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for assigning a role to a user
    /// </summary>
    public class AssignRoleDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }

    /// <summary>
    /// DTO for user with assigned roles
    /// </summary>
    public class UserRoleResponseDto
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
