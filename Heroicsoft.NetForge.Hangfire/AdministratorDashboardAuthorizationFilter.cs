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
public sealed class AdministratorDashboardAuthorizationFilter(string role = WellKnownRoles.Administrator)
    : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(role);
    }
}