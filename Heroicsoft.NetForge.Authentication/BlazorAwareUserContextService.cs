using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Resolves the caller from <c>HttpContext</c> when there is one, and from the Blazor
/// authentication state otherwise. Interactive Blazor Server components run on a SignalR
/// circuit where <c>HttpContext</c> is null, so an HTTP-only implementation would report every
/// component interaction as anonymous.
/// </summary>
public sealed class BlazorAwareUserContextService(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider authenticationStateProvider,
    IOptions<NetForgeAuthOptions> forgeOptions) : IUserContextService
{
    public string? GetCurrentUserId() => ResolvePrincipal().FindUserId();

    public string? GetCurrentUserName() => ResolvePrincipal()?.Identity?.Name;

    public bool IsAuthenticated() => ResolvePrincipal()?.Identity?.IsAuthenticated ?? false;

    public bool IsAdministrator()
    {
        var user = ResolvePrincipal();
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
        var user = ResolvePrincipal();
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

    private ClaimsPrincipal? ResolvePrincipal()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User;
        }

        try
        {
            // Sync-over-async is unavoidable here: IUserContextService is consumed from
            // synchronous property getters throughout the services, and the state provider's
            // task is already completed for an established circuit.
            var user = Task.Run(async () =>
                    (await authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false)).User)
                .GetAwaiter()
                .GetResult();

            return user.Identity?.IsAuthenticated == true ? user : httpContext?.User;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            return httpContext?.User;
        }
    }
}