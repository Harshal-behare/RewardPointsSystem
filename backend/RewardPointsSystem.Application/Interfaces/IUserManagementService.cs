using RewardPointsSystem.Application.Common;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Users;

namespace RewardPointsSystem.Application.Interfaces;

/// <summary>
/// Application layer service for user management operations.
/// Handles user creation workflows including password hashing, role assignment, and account creation.
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Creates a new user with full workflow: password hashing, role assignment, and points account creation.
    /// </summary>
    /// <param name="dto">User creation data</param>
    /// <returns>Result of the operation</returns>
    Task<Result<UserResponseDto>> CreateUserAsync(CreateUserDto dto);

    /// <summary>
    /// Updates a user's information including activation/deactivation.
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="dto">Update data</param>
    /// <returns>Result containing updated user or error</returns>
    Task<Result<UserResponseDto>> UpdateUserAsync(Guid userId, UpdateUserDto dto);

    /// <summary>
    /// Gets all users with pagination applied in the Application layer.
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated user list with total count</returns>
    Task<(IEnumerable<UserResponseDto> Users, int TotalCount)> GetUsersPagedAsync(int page, int pageSize);

    /// <summary>
    /// Deactivates a user.
    /// </summary>
    /// <param name="userId">User ID to deactivate</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> DeactivateUserAsync(Guid userId);
}

// Legacy result types - kept for backward compatibility, prefer using Result<T>
public class UserOperationResult
{
    public bool Success { get; set; }
    public UserResponseDto? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public UserOperationErrorType ErrorType { get; set; }

    public static UserOperationResult Succeeded(UserResponseDto data) => 
        new() { Success = true, Data = data };
    
    public static UserOperationResult Failed(string message, UserOperationErrorType errorType = UserOperationErrorType.ValidationError) => 
        new() { Success = false, ErrorMessage = message, ErrorType = errorType };
}

public enum UserOperationErrorType
{
    None,
    NotFound,
    Conflict,
    ValidationError,
    Unauthorized
}
