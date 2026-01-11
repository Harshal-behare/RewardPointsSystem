using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Users;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages user-related operations
    /// </summary>
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IUserPointsAccountService _accountService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IUserPointsAccountService accountService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination (Admin only)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
        /// <response code="200">Returns paginated user list</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">User lacks admin privileges</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                
                var userDtos = users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();

                // Apply pagination
                var totalCount = userDtos.Count;
                var pagedUsers = userDtos
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResponse = PagedResponse<UserResponseDto>.Create(pagedUsers, page, pageSize, totalCount);
                
                return PagedSuccess(pagedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return Error("Failed to retrieve users");
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <response code="200">Returns user details</response>
        /// <response code="404">User not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                    return NotFoundError($"User with ID {id} not found");

                var userDto = new UserResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return Error("Failed to retrieve user");
            }
        }

        /// <summary>
        /// Create a new user (Admin only)
        /// </summary>
        /// <param name="dto">User creation data</param>
        /// <response code="201">User created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="409">User with email already exists</response>
        /// <response code="422">Validation failed</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName);

                var userDto = new UserResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return Created(userDto, "User created successfully");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return ConflictError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return Error("Failed to create user");
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">User update data</param>
        /// <response code="200">User updated successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="422">Validation failed</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                    return NotFoundError($"User with ID {id} not found");

                // Create update DTO with merged values
                var updateDto = new RewardPointsSystem.Application.Interfaces.UserUpdateDto
                {
                    FirstName = dto.FirstName ?? user.FirstName,
                    LastName = dto.LastName ?? user.LastName,
                    Email = dto.Email ?? user.Email
                };

                await _userService.UpdateUserAsync(id, updateDto);

                var updatedUser = await _userService.GetUserByIdAsync(id);
                
                var userDto = new UserResponseDto
                {
                    Id = updatedUser.Id,
                    FirstName = updatedUser.FirstName,
                    LastName = updatedUser.LastName,
                    Email = updatedUser.Email,
                    IsActive = updatedUser.IsActive,
                    CreatedAt = updatedUser.CreatedAt
                };

                return Success(userDto, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return Error("Failed to update user");
            }
        }

        /// <summary>
        /// Delete user (soft delete - Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <response code="200">User deleted successfully</response>
        /// <response code="404">User not found</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                    return NotFoundError($"User with ID {id} not found");

                await _userService.DeactivateUserAsync(id);

                return Success<object>(null, "User deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return Error("Failed to delete user");
            }
        }

        /// <summary>
        /// Get user points balance
        /// </summary>
        /// <param name="id">User ID</param>
        /// <response code="200">Returns user balance</response>
        /// <response code="404">User or account not found</response>
        [HttpGet("{id}/balance")]
        [ProducesResponseType(typeof(ApiResponse<UserBalanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserBalance(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                    return NotFoundError($"User with ID {id} not found");

                var account = await _accountService.GetAccountAsync(id);

                var balanceDto = new UserBalanceDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    CurrentBalance = account?.CurrentBalance ?? 0,
                    TotalEarned = account?.TotalEarned ?? 0,
                    TotalRedeemed = account?.TotalRedeemed ?? 0,
                    LastTransaction = account?.LastUpdatedAt ?? user.CreatedAt
                };

                return Success(balanceDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Points account not found for user {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance for user {UserId}", id);
                return Error("Failed to retrieve user balance");
            }
        }
    }
}
