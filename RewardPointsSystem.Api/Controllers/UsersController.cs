using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.Common;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Users;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages user-related operations.
    /// Clean Architecture compliant - delegates business logic to Application layer services.
    /// </summary>
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IUserQueryService _userQueryService;
        private readonly IUserPointsAccountService _accountService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IUserQueryService userQueryService,
            IUserPointsAccountService accountService,
            IUserManagementService userManagementService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _userQueryService = userQueryService;
            _accountService = accountService;
            _userManagementService = userManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination including inactive users (Admin only)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
        /// <response code="200">Returns paginated user list including inactive users</response>
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
                var (users, totalCount) = await _userManagementService.GetUsersPagedAsync(page, pageSize);

                var pagedResponse = PagedResponse<UserResponseDto>.Create(users, page, pageSize, totalCount);
                
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
                var user = await _userQueryService.GetUserWithDetailsAsync(id);
                
                if (user == null)
                    return NotFoundError($"User with ID {id} not found");

                return Success(user);
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
            var result = await _userManagementService.CreateUserAsync(dto);

            if (result.IsSuccess)
                _logger.LogInformation("User {Email} created successfully by admin", dto.Email);

            return ToCreatedResult(result, "User created successfully");
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">User update data (raw DTO - no merging in API)</param>
        /// <response code="200">User updated successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="403">Not authorized to update this user</response>
        /// <response code="422">Validation failed</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            // Pass raw DTO to Application layer - no merging in API
            var result = await _userManagementService.UpdateUserAsync(id, dto);
            return ToActionResult(result, "User updated successfully");
        }

        /// <summary>
        /// Deactivate user (Admin only). Use PUT with isActive=false for more control.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <response code="200">User deactivated successfully</response>
        /// <response code="400">Cannot deactivate user (pending redemptions or last admin)</response>
        /// <response code="404">User not found</response>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            var result = await _userManagementService.DeactivateUserAsync(id);
            return ToActionResult(result, "User deactivated successfully");
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

                var balance = await _accountService.GetBalanceAsync(id);
                var account = await _accountService.GetAccountAsync(id);

                var balanceDto = new UserBalanceDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    CurrentBalance = balance,
                    TotalEarned = account.TotalEarned,
                    TotalRedeemed = account.TotalRedeemed,
                    LastTransaction = account.LastUpdatedAt
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
