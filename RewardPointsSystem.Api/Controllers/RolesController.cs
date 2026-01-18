using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IRoleService roleService,
            IUserRoleService userRoleService,
            ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _userRoleService = userRoleService;
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
                var roles = await _roleService.GetAllRolesAsync();
                var roleDtos = roles.Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt
                });

                return Success(roleDtos);
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
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                    return NotFoundError($"Role with ID {id} not found");

                var roleDto = new RoleResponseDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt
                };

                return Success(roleDto);
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
                var role = await _roleService.CreateRoleAsync(dto.Name, dto.Description);

                var roleDto = new RoleResponseDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt
                };

                return Created(roleDto, "Role created successfully");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return ConflictError(ex.Message);
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
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                    return NotFoundError($"Role with ID {id} not found");

                // Update role (assuming service has update method)
                role = await _roleService.UpdateRoleAsync(id, dto.Name, dto.Description);

                var roleDto = new RoleResponseDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt
                };

                return Success(roleDto, "Role updated successfully");
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
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                    return NotFoundError($"Role with ID {id} not found");

                await _roleService.DeleteRoleAsync(id);

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
                // Get admin user ID from JWT claims
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminUserId))
                    return UnauthorizedError("Admin user not authenticated");

                await _userRoleService.AssignRoleAsync(userId, dto.RoleId, adminUserId);
                return Success<object>(null, "Role assigned successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("User or role not found");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
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
                await _userRoleService.RevokeRoleAsync(userId, roleId);
                return Success<object>(null, "Role revoked successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("User or role not found");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
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
                var roles = await _userRoleService.GetUserRolesAsync(userId);
                var roleDtos = roles.Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt
                });

                return Success(roleDtos);
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
    }
}
