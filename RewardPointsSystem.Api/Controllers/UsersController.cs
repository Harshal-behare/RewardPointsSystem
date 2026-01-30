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
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IUserPointsAccountService accountService,
            IRoleService roleService,
            IUserRoleService userRoleService,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _accountService = accountService;
            _roleService = roleService;
            _userRoleService = userRoleService;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
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
                // Get ALL users (including inactive) for admin
                var users = await _userService.GetAllUsersAsync();
                
                // Get all user roles and points accounts for efficient lookup
                var allUserRoles = await _unitOfWork.UserRoles.GetAllAsync();
                var allRoles = await _unitOfWork.Roles.GetAllAsync();
                var allPointsAccounts = await _unitOfWork.UserPointsAccounts.GetAllAsync();
                
                var userDtos = users.Select(u => {
                    // Get roles for this user
                    var userRoleIds = allUserRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.RoleId).ToList();
                    var roleNames = allRoles.Where(r => userRoleIds.Contains(r.Id)).Select(r => r.Name).ToList();
                    
                    // Get points balance for this user
                    var pointsAccount = allPointsAccounts.FirstOrDefault(pa => pa.UserId == u.Id);
                    
                    return new UserResponseDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        Roles = roleNames,
                        PointsBalance = pointsAccount?.CurrentBalance
                    };
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
                // Create user
                var user = await _userService.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName);

                // Set password - use provided password or generate a random one
                var password = !string.IsNullOrEmpty(dto.Password) ? dto.Password : GenerateRandomPassword();
                var passwordHash = _passwordHasher.HashPassword(password);
                user.SetPasswordHash(passwordHash);
                
                // Update user to save password hash
                var updateDto = new Application.Interfaces.UserUpdateDto
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                await _userService.UpdateUserAsync(user.Id, updateDto);

                // Assign role (default: Employee)
                var roleName = !string.IsNullOrEmpty(dto.Role) ? dto.Role : "Employee";
                var role = await _roleService.GetRoleByNameAsync(roleName);
                if (role != null)
                {
                    await _userRoleService.AssignRoleAsync(user.Id, role.Id, user.Id);
                }

                // Create points account for the user
                await _accountService.CreateAccountAsync(user.Id);

                // Get assigned roles for response
                var assignedRoles = await _userRoleService.GetUserRolesAsync(user.Id);

                var userDto = new UserResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = assignedRoles.Select(r => r.Name).ToList(),
                    PointsBalance = 0
                };

                _logger.LogInformation("User {Email} created successfully by admin", user.Email);
                return Created(userDto, "User created successfully");
            }
            catch (RewardPointsSystem.Domain.Exceptions.DuplicateUserEmailException ex)
            {
                return ConflictError(ex.Message);
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

                // Handle activation/deactivation separately if isActive is provided
                if (dto.IsActive.HasValue)
                {
                    if (dto.IsActive.Value && !user.IsActive)
                    {
                        // Activate user - use User.Activate method
                        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        var activatedBy = Guid.TryParse(userIdClaim, out var activatorId) ? activatorId : id;
                        user.Activate(activatedBy);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    else if (!dto.IsActive.Value && user.IsActive)
                    {
                        // Deactivate user
                        await _userService.DeactivateUserAsync(id);
                    }
                }

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
        /// Deactivate user (Admin only). Use PUT with isActive=false for more control.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <response code="200">User deactivated successfully</response>
        /// <response code="404">User not found</response>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateUser(Guid id)
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
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                return Error("Failed to deactivate user");
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

        /// <summary>
        /// Generate a random password for new users
        /// </summary>
        private static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            var password = new char[12];
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            return new string(password);
        }
    }
}
