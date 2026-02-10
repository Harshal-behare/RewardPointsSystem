using RewardPointsSystem.Application.Common;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Users;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.UseCases.Users;

/// <summary>
/// Use case for updating user information including activation/deactivation.
/// Encapsulates all business logic related to user updates.
/// </summary>
public class UpdateUserUseCase
{
    private readonly IUserService _userService;
    private readonly IUserQueryService _userQueryService;
    private readonly ICurrentUserContext _currentUserContext;

    public UpdateUserUseCase(
        IUserService userService,
        IUserQueryService userQueryService,
        ICurrentUserContext currentUserContext)
    {
        _userService = userService;
        _userQueryService = userQueryService;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Updates a user's information and optionally their active status.
    /// Handles authorization, validation, and activation/deactivation logic.
    /// </summary>
    /// <param name="userId">The user ID to update</param>
    /// <param name="dto">The update data (raw DTO from API)</param>
    /// <returns>Result containing the updated user or error</returns>
    public async Task<Result<UserResponseDto>> ExecuteAsync(Guid userId, UpdateUserDto dto)
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

    /// <summary>
    /// Handles user activation or deactivation with all business rules.
    /// </summary>
    private async Task<Result> HandleActivationChangeAsync(Guid userId, bool currentStatus, bool newStatus, Guid performedBy)
    {
        if (currentStatus == newStatus)
            return Result.Success(); // No change needed

        if (newStatus)
        {
            // Activate user
            await _userService.ActivateUserAsync(userId, performedBy);
            return Result.Success();
        }
        else
        {
            // Deactivate user - this may throw if user has pending items or is last admin
            try
            {
                await _userService.DeactivateUserAsync(userId);
                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                // Convert domain exceptions to Result errors
                if (ex.Message.Contains("pending"))
                    return Result.BusinessRuleViolation(ex.Message);
                if (ex.Message.Contains("last") || ex.Message.Contains("admin"))
                    return Result.BusinessRuleViolation(ex.Message);
                return Result.Failure(ErrorType.BusinessRule, ex.Message);
            }
        }
    }
}
