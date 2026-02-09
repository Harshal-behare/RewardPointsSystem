using Microsoft.Extensions.Logging;
using RewardPointsSystem.Application.DTOs.Roles;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Roles
{
    /// <summary>
    /// Service implementation for role management operations.
    /// All business logic centralized here - Clean Architecture compliant.
    /// </summary>
    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<RoleManagementService> _logger;

        public RoleManagementService(
            IRoleService roleService,
            IUserRoleService userRoleService,
            ILogger<RoleManagementService> logger)
        {
            _roleService = roleService;
            _userRoleService = userRoleService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<RoleOperationResult> CreateRoleAsync(CreateRoleDto dto)
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

                _logger.LogInformation("Role {RoleName} created successfully", dto.Name);
                return RoleOperationResult.Succeeded(roleDto);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return RoleOperationResult.Failed(ex.Message, RoleOperationErrorType.Conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role {RoleName}", dto.Name);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RoleOperationResult> UpdateRoleAsync(Guid id, CreateRoleDto dto)
        {
            try
            {
                var existingRole = await _roleService.GetRoleByIdAsync(id);
                if (existingRole == null)
                {
                    return RoleOperationResult.Failed(
                        $"Role with ID {id} not found",
                        RoleOperationErrorType.NotFound);
                }

                var role = await _roleService.UpdateRoleAsync(id, dto.Name, dto.Description);

                var roleDto = new RoleResponseDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt
                };

                _logger.LogInformation("Role {RoleId} updated successfully", id);
                return RoleOperationResult.Succeeded(roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RoleOperationResult> DeleteRoleAsync(Guid id)
        {
            try
            {
                var existingRole = await _roleService.GetRoleByIdAsync(id);
                if (existingRole == null)
                {
                    return RoleOperationResult.Failed(
                        $"Role with ID {id} not found",
                        RoleOperationErrorType.NotFound);
                }

                await _roleService.DeleteRoleAsync(id);

                _logger.LogInformation("Role {RoleId} deleted successfully", id);
                return RoleOperationResult.Succeeded();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RoleOperationResult> AssignRoleToUserAsync(Guid userId, Guid roleId, Guid adminUserId)
        {
            try
            {
                await _userRoleService.AssignRoleAsync(userId, roleId, adminUserId);
                
                _logger.LogInformation(
                    "Role {RoleId} assigned to user {UserId} by admin {AdminId}",
                    roleId, userId, adminUserId);
                    
                return RoleOperationResult.Succeeded();
            }
            catch (KeyNotFoundException)
            {
                return RoleOperationResult.Failed(
                    "User or role not found",
                    RoleOperationErrorType.NotFound);
            }
            catch (InvalidOperationException ex)
            {
                return RoleOperationResult.Failed(ex.Message, RoleOperationErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", roleId, userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RoleOperationResult> RevokeRoleFromUserAsync(Guid userId, Guid roleId)
        {
            try
            {
                await _userRoleService.RevokeRoleAsync(userId, roleId);
                
                _logger.LogInformation("Role {RoleId} revoked from user {UserId}", roleId, userId);
                return RoleOperationResult.Succeeded();
            }
            catch (KeyNotFoundException)
            {
                return RoleOperationResult.Failed(
                    "User or role not found",
                    RoleOperationErrorType.NotFound);
            }
            catch (InvalidOperationException ex)
            {
                return RoleOperationResult.Failed(ex.Message, RoleOperationErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking role {RoleId} from user {UserId}", roleId, userId);
                throw;
            }
        }
    }
}
