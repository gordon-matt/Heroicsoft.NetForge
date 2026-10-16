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
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(IAuthProviderService authProvider) : Controller
{
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl) => authProvider.Provider != AuthProvider.Keycloak
        ? Redirect("/")
        : Challenge(
            new AuthenticationProperties { RedirectUri = SafeReturnUrl(returnUrl) },
            OpenIdConnectDefaults.AuthenticationScheme);

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