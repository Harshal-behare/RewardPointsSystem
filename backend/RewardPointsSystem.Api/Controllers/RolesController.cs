using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Roles;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages role and permission operations
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class RolesController : BaseApiController
    {
        private readonly IRoleQueryService _roleQueryService;
        private readonly IRoleManagementService _roleManagementService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IRoleQueryService roleQueryService,
            IRoleManagementService roleManagementService,
            ILogger<RolesController> logger)
        {
            _roleQueryService = roleQueryService;
            _roleManagementService = roleManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        /// <response code="200">Returns list of all roles</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _roleQueryService.GetAllRolesAsync();
                return Success(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return Error("Failed to retrieve roles");
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <response code="200">Returns role details</response>
        /// <response code="404">Role not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            try
            {
                var role = await _roleQueryService.GetRoleByIdAsync(id);
                if (role == null)
                    return NotFoundError($"Role with ID {id} not found");

                return Success(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId}", id);
                return Error("Failed to retrieve role");
            }
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="dto">Role creation data</param>
        /// <response code="201">Role created successfully</response>
        /// <response code="409">Role name already exists</response>
        /// <response code="422">Validation failed</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                var result = await _roleManagementService.CreateRoleAsync(dto);

                if (!result.Success)
                {
                    return MapRoleErrorToResponse(result);
                }

                return Created(result.Data!, "Role created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return Error("Failed to create role");
            }
        }

        /// <summary>
        /// Update role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="dto">Role update data</param>
        /// <response code="200">Role updated successfully</response>
        /// <response code="404">Role not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] CreateRoleDto dto)
        {
            try
            {
                var result = await _roleManagementService.UpdateRoleAsync(id, dto);

                if (!result.Success)
                {
                    return MapRoleErrorToResponse(result);
                }

                return Success(result.Data!, "Role updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", id);
                return Error("Failed to update role");
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <response code="200">Role deleted successfully</response>
        /// <response code="404">Role not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            try
            {
                var result = await _roleManagementService.DeleteRoleAsync(id);

                if (!result.Success)
                {
                    return MapRoleErrorToResponse(result);
                }

                return Success<object>(null, "Role deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                return Error("Failed to delete role");
            }
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">Role assignment data</param>
        /// <response code="200">Role assigned successfully</response>
        /// <response code="404">User or role not found</response>
        [HttpPost("users/{userId}/assign")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] AssignRoleDto dto)
        {
            try
            {
                var adminUserId = GetCurrentUserId();
                if (!adminUserId.HasValue)
                    return UnauthorizedError("Admin user not authenticated");

                var result = await _roleManagementService.AssignRoleToUserAsync(userId, dto.RoleId, adminUserId.Value);

                if (!result.Success)
                {
                    return MapRoleErrorToResponse(result);
                }

                return Success<object>(null, "Role assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}", userId);
                return Error("Failed to assign role");
            }
        }

        /// <summary>
        /// Revoke role from user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <response code="200">Role revoked successfully</response>
        /// <response code="404">User or role not found</response>
        [HttpDelete("users/{userId}/roles/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeRoleFromUser(Guid userId, Guid roleId)
        {
            try
            {
                var result = await _roleManagementService.RevokeRoleFromUserAsync(userId, roleId);

                if (!result.Success)
                {
                    return MapRoleErrorToResponse(result);
                }

                return Success<object>(null, "Role revoked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking role from user {UserId}", userId);
                return Error("Failed to revoke role");
            }
        }

        /// <summary>
        /// Get user's roles
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <response code="200">Returns user's roles</response>
        /// <response code="404">User not found</response>
        [HttpGet("users/{userId}/roles")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles(Guid userId)
        {
            try
            {
                var roles = await _roleQueryService.GetUserRolesAsync(userId);
                return Success(roles);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"User with ID {userId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user roles for {UserId}", userId);
                return Error("Failed to retrieve user roles");
            }
        }

        /// <summary>
        /// Maps role operation result errors to appropriate HTTP responses
        /// </summary>
        private IActionResult MapRoleErrorToResponse(RoleOperationResult result)
        {
            return result.ErrorType switch
            {
                RoleOperationErrorType.NotFound => NotFoundError(result.ErrorMessage!),
                RoleOperationErrorType.Conflict => ConflictError(result.ErrorMessage!),
                RoleOperationErrorType.Unauthorized => UnauthorizedError(result.ErrorMessage!),
                _ => Error(result.ErrorMessage!, 400)
            };
        }
    }
}
