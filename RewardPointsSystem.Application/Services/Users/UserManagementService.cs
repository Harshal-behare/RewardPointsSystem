using RewardPointsSystem.Application.Common;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Users;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Application.Services.Users;

/// <summary>
/// Application layer service for user management operations.
/// Encapsulates user creation workflow that was previously in the controller.
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly IUserService _userService;
    private readonly IUserQueryService _userQueryService;
    private readonly IUserPointsAccountService _accountService;
    private readonly IRoleService _roleService;
    private readonly IUserRoleService _userRoleService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserContext _currentUserContext;

    public UserManagementService(
        IUserService userService,
        IUserQueryService userQueryService,
        IUserPointsAccountService accountService,
        IRoleService roleService,
        IUserRoleService userRoleService,
        IPasswordHasher passwordHasher,
        ICurrentUserContext currentUserContext)
    {
        _userService = userService;
        _userQueryService = userQueryService;
        _accountService = accountService;
        _roleService = roleService;
        _userRoleService = userRoleService;
        _passwordHasher = passwordHasher;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result<UserResponseDto>> CreateUserAsync(CreateUserDto dto)
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
            var updateDto = new Interfaces.UserUpdateDto
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

            return Result<UserResponseDto>.Success(userDto);
        }
        catch (DuplicateUserEmailException ex)
        {
            return Result<UserResponseDto>.Conflict(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Result<UserResponseDto>.Conflict(ex.Message);
        }
    }

    public async Task<Result<UserResponseDto>> UpdateUserAsync(Guid userId, UpdateUserDto dto)
    {
        // Get current user for authorization
        var currentUserId = _currentUserContext.UserId;
        if (!currentUserId.HasValue)
            return Result<UserResponseDto>.Unauthorized("User not authenticated");

        // Get existing user
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return Result<UserResponseDto>.NotFound($"User with ID {userId} not found");

        // Check authorization: users can update themselves, admins can update anyone
        var isAdmin = _currentUserContext.IsInRole("Admin");
        if (userId != currentUserId.Value && !isAdmin)
            return Result<UserResponseDto>.Forbidden("You can only update your own profile");

        // Non-admins cannot change activation status
        if (dto.IsActive.HasValue && !isAdmin)
            return Result<UserResponseDto>.Forbidden("Only administrators can change user activation status");

        // Apply profile updates (merge in Application layer, not API)
        var updateDto = new Interfaces.UserUpdateDto
        {
            FirstName = dto.FirstName ?? user.FirstName,
            LastName = dto.LastName ?? user.LastName,
            Email = dto.Email ?? user.Email
        };

        await _userService.UpdateUserAsync(userId, updateDto);

        // Handle activation/deactivation
        if (dto.IsActive.HasValue)
        {
            var activationResult = await HandleActivationChangeAsync(userId, user.IsActive, dto.IsActive.Value, currentUserId.Value);
            if (activationResult.IsFailure)
                return Result<UserResponseDto>.FromResult(activationResult);
        }

        // Return updated user
        var updatedUser = await _userQueryService.GetUserWithDetailsAsync(userId);
        return Result<UserResponseDto>.Success(updatedUser!);
    }

    public async Task<Result> DeactivateUserAsync(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return Result.NotFound($"User with ID {userId} not found");

        try
        {
            await _userService.DeactivateUserAsync(userId);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.BusinessRuleViolation(ex.Message);
        }
    }

    public async Task<(IEnumerable<UserResponseDto> Users, int TotalCount)> GetUsersPagedAsync(int page, int pageSize)
    {
        var users = await _userQueryService.GetAllUsersWithDetailsAsync();
        var userList = users.ToList();

        // Apply pagination
        var totalCount = userList.Count;
        var pagedUsers = userList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedUsers, totalCount);
    }

    private async Task<Result> HandleActivationChangeAsync(Guid userId, bool currentStatus, bool newStatus, Guid performedBy)
    {
        if (currentStatus == newStatus)
            return Result.Success();

        if (newStatus)
        {
            await _userService.ActivateUserAsync(userId, performedBy);
            return Result.Success();
        }
        else
        {
            try
            {
                await _userService.DeactivateUserAsync(userId);
                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                return Result.BusinessRuleViolation(ex.Message);
            }
        }
    }

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
