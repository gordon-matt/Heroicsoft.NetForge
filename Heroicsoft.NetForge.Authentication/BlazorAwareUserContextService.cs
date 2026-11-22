using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Resolves the caller from <see cref="HttpContext"/> when that user is authenticated, and from
/// the Blazor authentication state otherwise. Interactive Blazor Server components run on a
/// SignalR circuit where <see cref="HttpContext"/> is <see langword="null"/>, so an HTTP-only
/// implementation would report every component interaction as anonymous. If the Blazor user is
/// not authenticated, or reading the authentication state throws <see cref="InvalidOperationException"/>
/// or <see cref="ObjectDisposedException"/>, the HTTP user is returned.
/// </summary>
/// <param name="httpContextAccessor">Source of the current HTTP principal.</param>
/// <param name="authenticationStateProvider">Blazor authentication state used when the HTTP user is missing or not authenticated.</param>
/// <param name="forgeOptions">Supplies <see cref="NetForgeAuthOptions.AdministratorRole"/>.</param>
public sealed class BlazorAwareUserContextService(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider authenticationStateProvider,
    IOptions<NetForgeAuthOptions> forgeOptions) : IUserContextService
{
    /// <inheritdoc/>
    public string? GetCurrentUserId() => ResolvePrincipal().FindUserId();

    /// <inheritdoc/>
    public string? GetCurrentUserName() => ResolvePrincipal()?.Identity?.Name;

    /// <inheritdoc/>
    public bool IsAuthenticated() => ResolvePrincipal()?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Returns <see langword="true"/> when the current principal is in
    /// <see cref="NetForgeAuthOptions.AdministratorRole"/>, or a collected role name matches that
    /// role without case. Returns <see langword="false"/> when there is no principal.
    /// </summary>
    /// <returns><see langword="true"/> when the caller has the administrator role.</returns>
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

    /// <summary>
    /// Returns the current principal's role names. Values that differ only by case are returned once.
    /// A claim is included when its type is <see cref="ClaimTypes.Role"/>, <c>role</c>, <c>roles</c>,
    /// or the identity's role claim type. Returns an empty list when there is no principal.
    /// </summary>
    /// <returns>The caller's role names.</returns>
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