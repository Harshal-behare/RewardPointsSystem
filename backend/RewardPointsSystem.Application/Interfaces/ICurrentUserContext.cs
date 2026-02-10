namespace RewardPointsSystem.Application.Interfaces;

/// <summary>
/// Abstraction for accessing the current authenticated user's context.
/// This allows Application layer services to access user identity without
/// depending on ASP.NET Core or HTTP-specific classes.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Gets the current user's ID from the authentication context.
    /// Returns null if user is not authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's email from the authentication context.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the current user has the specified role.
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if user has the role</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Gets all roles assigned to the current user.
    /// </summary>
    IEnumerable<string> Roles { get; }
}
