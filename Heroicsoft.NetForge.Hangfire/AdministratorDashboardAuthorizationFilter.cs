using Hangfire.Dashboard;
using Heroicsoft.NetForge.Abstractions.Auth;

namespace Heroicsoft.NetForge.Hangfire;

/// <summary>
/// Restricts the Hangfire dashboard to authenticated administrators.
/// </summary>
/// <remarks>
/// Hangfire's dashboard is unauthenticated by default and lets a visitor enqueue and delete
/// jobs, so an explicit filter is required rather than optional. Pass an instance:
/// <c>Authorization = [new AdministratorDashboardAuthorizationFilter()]</c>.
/// </remarks>
/// <param name="role">The role required to open the dashboard. The default is <see cref="WellKnownRoles.Administrator"/>.</param>
public sealed class AdministratorDashboardAuthorizationFilter(string role = WellKnownRoles.Administrator)
    : IDashboardAuthorizationFilter
{
    /// <summary>
    /// Returns whether the current user is authenticated and in the role passed to the constructor.
    /// </summary>
    /// <param name="context">The dashboard request.</param>
    /// <returns><see langword="true"/> when the user is signed in and has that role.</returns>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(role);
    }
}