using RewardPointsSystem.Application.DTOs.Roles;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Service for role management operations.
    /// Part of Clean Architecture pattern - encapsulates all business logic.
    /// </summary>
    public interface IRoleManagementService
    {
        /// <summary>
        /// Creates a new role
        /// </summary>
        Task<RoleOperationResult> CreateRoleAsync(CreateRoleDto dto);

        /// <summary>
        /// Updates an existing role
        /// </summary>
        Task<RoleOperationResult> UpdateRoleAsync(Guid id, CreateRoleDto dto);

        /// <summary>
        /// Deletes a role
        /// </summary>
        Task<RoleOperationResult> DeleteRoleAsync(Guid id);

        /// <summary>
        /// Assigns a role to a user
        /// </summary>
        Task<RoleOperationResult> AssignRoleToUserAsync(Guid userId, Guid roleId, Guid adminUserId);

        /// <summary>
        /// Revokes a role from a user
        /// </summary>
        Task<RoleOperationResult> RevokeRoleFromUserAsync(Guid userId, Guid roleId);
    }

    /// <summary>
    /// Result of a role operation
    /// </summary>
    public class RoleOperationResult
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }
        public RoleOperationErrorType ErrorType { get; private set; }
        public RoleResponseDto? Data { get; private set; }

        private RoleOperationResult() { }

        public static RoleOperationResult Succeeded(RoleResponseDto? data = null)
        {
            return new RoleOperationResult
            {
                Success = true,
                Data = data
            };
        }

        public static RoleOperationResult Failed(string errorMessage, RoleOperationErrorType errorType = RoleOperationErrorType.ValidationError)
        {
            return new RoleOperationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorType = errorType
            };
        }
    }

    public enum RoleOperationErrorType
    {
        ValidationError,
        NotFound,
        Conflict,
        Unauthorized
    }
}
