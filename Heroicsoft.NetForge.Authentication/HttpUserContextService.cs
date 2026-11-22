using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Resolves the caller from <see cref="HttpContext"/>. MVC requests always have one. Blazor
/// circuits do not, so those hosts get <see cref="BlazorAwareUserContextService"/> instead.
/// </summary>
/// <param name="httpContextAccessor">Source of the current HTTP principal.</param>
/// <param name="forgeOptions">Supplies <see cref="NetForgeAuthOptions.AdministratorRole"/>.</param>
public sealed class HttpUserContextService(
    IHttpContextAccessor httpContextAccessor,
    IOptions<NetForgeAuthOptions> forgeOptions) : IUserContextService
{
    /// <inheritdoc/>
    public string? GetCurrentUserId() => User.FindUserId();

    /// <inheritdoc/>
    public string? GetCurrentUserName() => User?.Identity?.Name;

    /// <inheritdoc/>
    public bool IsAuthenticated() => User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Returns <see langword="true"/> when the current principal is in
    /// <see cref="NetForgeAuthOptions.AdministratorRole"/>, or a collected role name matches that
    /// role without case. Returns <see langword="false"/> when there is no principal.
    /// </summary>
    /// <returns><see langword="true"/> when the caller has the administrator role.</returns>
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
    /// Returns the current principal's role names. Values that differ only by case are returned once.
    /// A claim is included when its type is <see cref="ClaimTypes.Role"/>, <c>role</c>, <c>roles</c>,
    /// or the identity's role claim type. Returns an empty list when there is no principal.
    /// </summary>
    /// <returns>The caller's role names.</returns>
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