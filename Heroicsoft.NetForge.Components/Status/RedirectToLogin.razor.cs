using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Status;

public partial class RedirectToLogin
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IAuthProviderService AuthProviderService { get; set; } = default!;

    // The login URL differs per provider, so it is resolved here rather than hard-coded at every
    // call site: Identity serves its own Razor Pages, while Keycloak goes through the challenge
    // endpoint that redirects to the IdP.
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