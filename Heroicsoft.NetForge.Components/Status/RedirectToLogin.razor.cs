using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Status;

/// <summary>
/// Sends the browser to the active provider's sign-in page and renders nothing. The current path is the return address.
/// </summary>
public partial class RedirectToLogin
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IAuthProviderService AuthProviderService { get; set; } = default!;

    // The login URL differs per provider, so it is resolved here rather than hard-coded at every
    // call site: Identity serves its own Razor Pages, while Keycloak goes through the challenge
    // endpoint that redirects to the IdP.
    /// <summary>
    /// Navigates with a full page load. Keycloak uses <c>auth/login</c>, Identity uses <c>Identity/Account/Login</c>, and any other provider goes to <c>/</c>.
    /// </summary>
    protected override void OnInitialized()
    {
        string returnUrl = Uri.EscapeDataString(
            Navigation.ToBaseRelativePath(Navigation.Uri) is { Length: > 0 } relative
                ? $"/{relative}"
                : "/");

        string loginUrl = AuthProviderService.Provider switch
        {
            AuthProvider.Keycloak => $"auth/login?returnUrl={returnUrl}",
            AuthProvider.Identity => $"Identity/Account/Login?ReturnUrl={returnUrl}",
            _ => "/",
        };

        // forceLoad, because both destinations are outside the Blazor router.
        Navigation.NavigateTo(loginUrl, forceLoad: true);
    }
}