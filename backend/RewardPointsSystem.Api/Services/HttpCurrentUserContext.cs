using System.Security.Claims;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Services;

/// <summary>
/// HTTP-based implementation of ICurrentUserContext.
/// Extracts user context from the current HTTP request's JWT claims.
/// </summary>
public class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return null;
            return userId;
        }
    }

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

    public IEnumerable<string> Roles
    {
        get
        {
            var roleClaims = User?.FindAll(ClaimTypes.Role) ?? Enumerable.Empty<Claim>();
            return roleClaims.Select(c => c.Value);
        }
    }
}
