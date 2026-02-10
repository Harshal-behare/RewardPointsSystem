using RewardPointsSystem.Application.DTOs.Roles;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Query service for role-related operations with DTO mapping.
    /// Part of Clean Architecture pattern - Application layer handles all mapping logic.
    /// </summary>
    public interface IRoleQueryService
    {
        /// <summary>
        /// Gets all roles as DTOs
        /// </summary>
        Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync();

        /// <summary>
        /// Gets a role by ID as DTO
        /// </summary>
        Task<RoleResponseDto?> GetRoleByIdAsync(Guid id);

        /// <summary>
        /// Gets user's roles as DTOs
        /// </summary>
        Task<IEnumerable<RoleResponseDto>> GetUserRolesAsync(Guid userId);
    }
}
