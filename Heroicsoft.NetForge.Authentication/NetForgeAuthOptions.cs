using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Bound from the <c>Authentication</c> configuration section. Property names match the
/// appsettings already used by the self-hosted apps.
/// </summary>
public sealed class NetForgeAuthOptions
{
    public const string SectionName = "Authentication";

    /// <summary><see cref="AuthProviderNames.Identity"/>, <see cref="AuthProviderNames.Keycloak"/>, or <see cref="AuthProviderNames.None"/>.</summary>
    public string Provider { get; set; } = AuthProviderNames.Identity;

    public string AdministratorRole { get; set; } = WellKnownRoles.Administrator;

    public string UserRole { get; set; } = WellKnownRoles.User;

    public string AccessDeniedPath { get; set; } = "/access-denied";

    /// <summary>
    /// Where a failed Keycloak callback is sent. Defaults to <see cref="AccessDeniedPath"/>.
    /// MVC apps that already have <c>/Home/Error</c> pass that path.
    /// </summary>
    public string? RemoteFailurePath { get; set; }

    /// <summary>
    /// When false, the host keeps its own <c>/auth</c> controller. The NetForge controller is
    /// removed even if the MVC SDK discovered it, because two controllers on that route make
    /// login ambiguous.
    /// </summary>
    public bool IncludeAuthController { get; set; } = true;

    /// <summary>
    /// Registers <see cref="IUserContextService"/>. Turn this off when the host already has its
    /// own user-context service.
    /// </summary>
    public bool RegisterUserContext { get; set; } = true;

    public IdentityAuthOptions Identity { get; set; } = new();

    public NoneAuthOptions None { get; set; } = new();

    public KeycloakAuthOptions Keycloak { get; set; } = new();
}

/// <summary>
/// Relaxed from the Identity defaults deliberately: these apps are run by the person who
/// installed them, and a complexity policy on a single-user instance is friction without a
/// corresponding threat.
/// </summary>
public sealed class IdentityAuthOptions
{
    /// <summary>
    /// When true, password rules are relaxed for a self-hosted single-user install. MVC apps
    /// that already rely on the Identity defaults set this to false.
    /// </summary>
    public bool UseRelaxedPasswordPolicy { get; set; } = true;

    public bool RequireDigit { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireUppercase { get; set; }

    public bool RequireNonAlphanumeric { get; set; }

    public int RequiredLength { get; set; } = 6;

    public bool RequireUniqueEmail { get; set; }

    public bool RequireConfirmedAccount { get; set; }
}

/// <summary>The synthetic user created when <see cref="AuthProviderNames.None"/> is selected.</summary>
public sealed class NoneAuthOptions
{
    public string UserId { get; set; } = "_default";

    public string UserName { get; set; } = "Default User";
}

public sealed class KeycloakAuthOptions
{
    public string? Authority { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? AdminClientId { get; set; }

    public string? AdminClientSecret { get; set; }

    /// <summary>
    /// Browser-facing origin written into the OIDC redirect URI when the app itself is reached
    /// through a different host than the one it binds to.
    /// </summary>
    public string? PublicOrigin { get; set; }

    /// <summary>
    /// Internal base URL for discovery, token, and userinfo calls. Leave empty when the
    /// authority is reachable from the app process.
    /// </summary>
    public string? BackchannelBaseUrl { get; set; }

    public string LoginPath { get; set; } = "/auth/login";

    public int CookieDays { get; set; } = 7;

    /// <summary>Blazor apps leave this true. HTTP Keycloak in Docker sets it false.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Lax matches the Blazor apps. MVC apps behind a reverse proxy use None, because Keycloak's
    /// cross-site POST callback would otherwise drop the correlation cookie.
    /// </summary>
    public SameSiteMode CorrelationCookieSameSite { get; set; } = SameSiteMode.Lax;

    public CookieSecurePolicy CorrelationCookieSecurePolicy { get; set; } = CookieSecurePolicy.SameAsRequest;

    /// <summary>
    /// When discovery was fetched through the back-channel, send the browser to the public
    /// authority instead of the internal host that discovery returned.
    /// </summary>
    public bool RedirectBrowserToPublicAuthority { get; set; }

    /// <summary>Runs after realm roles have been copied onto the principal. Used to provision a local user row.</summary>
    public Func<TokenValidatedContext, Task>? OnTokenValidated { get; set; }
}