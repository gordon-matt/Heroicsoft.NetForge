using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Resolves the caller from <see cref="HttpContext"/>. MVC requests always have one. Blazor
/// circuits do not, so those hosts get <see cref="BlazorAwareUserContextService"/> instead.
/// </summary>
public sealed class HttpUserContextService(
    IHttpContextAccessor httpContextAccessor,
    IOptions<NetForgeAuthOptions> forgeOptions) : IUserContextService
{
    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    /// <returns>The current user identifier.</returns>
    public string? GetCurrentUserId() => User.FindUserId();

    /// <summary>
    /// Gets the current user name.
    /// </summary>
    /// <returns>The current user name.</returns>
    public string? GetCurrentUserName() => User?.Identity?.Name;

    /// <summary>
    /// Determines whether this instance is authenticated.
    /// </summary>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    public bool IsAuthenticated() => User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Determines whether this instance is administrator.
    /// </summary>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    public bool IsAdministrator()
    {
        var user = User;
        if (user is null)
        {
            return false;
        }

        string role = forgeOptions.Value.AdministratorRole;
        return user.IsInRole(role)
            || GetRoleNames().Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the role names.
    /// </summary>
    /// <returns>The role names.</returns>
    public IReadOnlyList<string> GetRoleNames()
    {
        var user = User;
        if (user is null)
        {
            return [];
        }

        var types = new HashSet<string>(StringComparer.Ordinal)
        {
            ClaimTypes.Role,
            "role",
            "roles",
        };

        if (user.Identity is ClaimsIdentity identity)
        {
            types.Add(identity.RoleClaimType);
        }

        return user.Claims
            .Where(claim => types.Contains(claim.Type))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}