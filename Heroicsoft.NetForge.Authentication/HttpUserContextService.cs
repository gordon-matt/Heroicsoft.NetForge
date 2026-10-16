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
    public string? GetCurrentUserId() => User.FindUserId();

    public string? GetCurrentUserName() => User?.Identity?.Name;

    public bool IsAuthenticated() => User?.Identity?.IsAuthenticated ?? false;

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