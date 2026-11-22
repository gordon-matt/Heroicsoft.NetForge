using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Heroicsoft.NetForge.Authentication.Controllers;

/// <summary>
/// Sign-in and sign-out for the Keycloak provider. Under ASP.NET Identity these are Razor Pages
/// supplied by <c>Microsoft.AspNetCore.Identity.UI</c>, and under "None" there is nothing to
/// sign into, so these endpoints only do anything in Keycloak mode.
/// </summary>
/// <param name="authProvider">Active auth mode. The actions redirect home when it is not Keycloak.</param>
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(IAuthProviderService authProvider) : Controller
{
    /// <summary>
    /// Starts a Keycloak sign-in and returns the browser to <paramref name="returnUrl"/> when that
    /// URL is local. Redirects to <c>/</c> when the provider is not <see cref="AuthProvider.Keycloak"/>,
    /// and uses <c>/</c> when <paramref name="returnUrl"/> is missing or not a local URL.
    /// </summary>
    /// <param name="returnUrl">Address to open after sign-in. Absolute and other non-local URLs are ignored.</param>
    /// <returns>A challenge for the OpenID Connect scheme, or a redirect to <c>/</c>.</returns>
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl) => authProvider.Provider != AuthProvider.Keycloak
        ? Redirect("/")
        : Challenge(
            new AuthenticationProperties { RedirectUri = SafeReturnUrl(returnUrl) },
            OpenIdConnectDefaults.AuthenticationScheme);

    /// <summary>
    /// Signs out the application cookie and the OpenID Connect session, then redirects to <c>/</c>.
    /// Redirects to <c>/</c> without signing out when the provider is not <see cref="AuthProvider.Keycloak"/>.
    /// The request must include a valid antiforgery token.
    /// </summary>
    /// <returns>A sign-out of both schemes, or a redirect to <c>/</c>.</returns>
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        if (authProvider.Provider != AuthProvider.Keycloak)
        {
            return Redirect("/");
        }

        // Both schemes: the cookie ends the local session, and the OIDC sign-out ends the one
        // at Keycloak. Without the second, clicking "sign out" then "sign in" would silently
        // put the user straight back in.
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Keeps <c>returnUrl</c> local. An absolute URL here would turn the login endpoint into an
    /// open redirect that a phishing link could point anywhere.
    /// </summary>
    private string SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}