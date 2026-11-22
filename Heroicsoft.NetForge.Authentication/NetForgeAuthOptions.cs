using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Bound from the <c>Authentication</c> configuration section. Property names match the
/// appsettings already used by the self-hosted apps.
/// </summary>
public sealed class NetForgeAuthOptions
{
    /// <summary>Name of the configuration section these options bind from, <c>Authentication</c>.</summary>
    public const string SectionName = "Authentication";

    /// <summary><see cref="AuthProviderNames.Identity"/>, <see cref="AuthProviderNames.Keycloak"/>, or <see cref="AuthProviderNames.None"/>.</summary>
    public string Provider { get; set; } = AuthProviderNames.Identity;

    /// <summary>
    /// Role name <see cref="HttpUserContextService"/> and <see cref="BlazorAwareUserContextService"/>
    /// compare in <c>IsAdministrator</c>. Defaults to <see cref="WellKnownRoles.Administrator"/>.
    /// <see cref="AuthProvider.None"/> grants this role to the synthetic user.
    /// </summary>
    public string AdministratorRole { get; set; } = WellKnownRoles.Administrator;

    /// <summary>
    /// Role name for an ordinary signed-in user. Defaults to <see cref="WellKnownRoles.User"/>.
    /// <see cref="AuthProvider.None"/> grants this role to the synthetic user.
    /// </summary>
    public string UserRole { get; set; } = WellKnownRoles.User;

    /// <summary>
    /// Path assigned to the Keycloak cookie handler when access is denied. Also the redirect for a
    /// failed Keycloak callback when <see cref="RemoteFailurePath"/> is <see langword="null"/>.
    /// Defaults to <c>/access-denied</c>.
    /// </summary>
    public string AccessDeniedPath { get; set; } = "/access-denied";

    /// <summary>
    /// Where a failed Keycloak callback is sent. Defaults to <see cref="AccessDeniedPath"/>.
    /// MVC apps that already have <c>/Home/Error</c> pass that path.
    /// </summary>
    public string? RemoteFailurePath { get; set; }

    /// <summary>
    /// When <see langword="false"/>, the host keeps its own <c>/auth</c> controller. The NetForge controller is
    /// removed even if the MVC SDK discovered it, because two controllers on that route make
    /// login ambiguous.
    /// </summary>
    public bool IncludeAuthController { get; set; } = true;

    /// <summary>
    /// Registers <see cref="IUserContextService"/>. Turn this off when the host already has its
    /// own user-context service.
    /// </summary>
    public bool RegisterUserContext { get; set; } = true;

    /// <summary>Password and sign-in options used when <see cref="Provider"/> is <see cref="AuthProviderNames.Identity"/>.</summary>
    public IdentityAuthOptions Identity { get; set; } = new();

    /// <summary>Synthetic user used when <see cref="Provider"/> is <see cref="AuthProviderNames.None"/>.</summary>
    public NoneAuthOptions None { get; set; } = new();

    /// <summary>Keycloak settings used when <see cref="Provider"/> is <see cref="AuthProviderNames.Keycloak"/>.</summary>
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
    /// When <see langword="true"/>, the other properties on this type replace the Identity password,
    /// user, and sign-in options for a self-hosted single-user install. MVC apps that already rely
    /// on the Identity defaults set this to <see langword="false"/>, and those properties are then ignored.
    /// </summary>
    public bool UseRelaxedPasswordPolicy { get; set; } = true;

    /// <summary>
    /// Whether a password must contain a digit. Copied onto Identity only when
    /// <see cref="UseRelaxedPasswordPolicy"/> is <see langword="true"/>. Defaults to <see langword="false"/>.
    /// </summary>
    public bool RequireDigit { get; set; }

    /// <summary>
    /// Whether a password must contain a lowercase letter. Copied onto Identity only when
    /// <see cref="UseRelaxedPasswordPolicy"/> is <see langword="true"/>. Defaults to <see langword="false"/>.
    /// </summary>
    public bool RequireLowercase { get; set; }

    /// <summary>
    /// Whether a password must contain an uppercase letter. Copied onto Identity only when
    /// <see cref="UseRelaxedPasswordPolicy"/> is <see langword="true"/>. Defaults to <see langword="false"/>.
    /// </summary>
    public bool RequireUppercase { get; set; }

    /// <summary>
    /// Whether a password must contain a character that is not a letter or digit. Copied onto Identity
    /// only when <see cref="UseRelaxedPasswordPolicy"/> is <see langword="true"/>. Defaults to <see langword="false"/>.
    /// </summary>
    public bool RequireNonAlphanumeric { get; set; }

    /// <summary>
    /// Minimum password length. Copied onto Identity only when <see cref="UseRelaxedPasswordPolicy"/>
    /// is <see langword="true"/>. Defaults to 6.
    /// </summary>
    public int RequiredLength { get; set; } = 6;

    /// <summary>
    /// Whether each user must have a distinct email address. Copied onto Identity only when
    /// <see cref="UseRelaxedPasswordPolicy"/> is <see langword="true"/>. Defaults to <see langword="false"/>.
    /// </summary>
    public bool RequireUniqueEmail { get; set; }

    /// <summary>
    /// Whether sign-in requires a confirmed account. Copied onto Identity only when
    /// <see cref="UseRelaxedPasswordPolicy"/> is <see langword="true"/>. Defaults to <see langword="false"/>.
    /// </summary>
    public bool RequireConfirmedAccount { get; set; }
}

/// <summary>The synthetic user created when <see cref="AuthProviderNames.None"/> is selected.</summary>
public sealed class NoneAuthOptions
{
    /// <summary>Name identifier of the synthetic user. Defaults to <c>_default</c>.</summary>
    public string UserId { get; set; } = "_default";

    /// <summary>Display name of the synthetic user. Defaults to <c>Default User</c>.</summary>
    public string UserName { get; set; } = "Default User";
}

/// <summary>Settings for OpenID Connect sign-in against Keycloak.</summary>
public sealed class KeycloakAuthOptions
{
    /// <summary>
    /// Browser-facing Keycloak authority URL. Registration throws <see cref="InvalidOperationException"/>
    /// when the provider is Keycloak and this is blank.
    /// </summary>
    public string? Authority { get; set; }

    /// <summary>OpenID Connect client id sent to Keycloak.</summary>
    public string? ClientId { get; set; }

    /// <summary>OpenID Connect client secret sent to Keycloak.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Optional client id for Keycloak admin calls made by the host. Authentication registration does not read this value.
    /// </summary>
    public string? AdminClientId { get; set; }

    /// <summary>
    /// Optional client secret paired with <see cref="AdminClientId"/>. Authentication registration does not read this value.
    /// </summary>
    public string? AdminClientSecret { get; set; }

    /// <summary>
    /// Browser-facing origin written into the OIDC redirect URI when the app itself is reached
    /// through a different host than the one it binds to. Sign-out uses it as the origin of
    /// <c>/signout-callback-oidc</c>.
    /// </summary>
    public string? PublicOrigin { get; set; }

    /// <summary>
    /// Internal base URL for discovery, token, and userinfo calls. Leave empty when the
    /// authority is reachable from the app process.
    /// </summary>
    public string? BackchannelBaseUrl { get; set; }

    /// <summary>Cookie login path. A challenge redirects here. Defaults to <c>/auth/login</c>.</summary>
    public string LoginPath { get; set; } = "/auth/login";

    /// <summary>
    /// Days until the authentication cookie expires. Values below 1 are treated as 1 day, and the
    /// cookie uses sliding expiration. Defaults to 7.
    /// </summary>
    public int CookieDays { get; set; } = 7;

    /// <summary>Blazor apps leave this <see langword="true"/>. HTTP Keycloak in Docker sets it <see langword="false"/>.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// <see cref="SameSiteMode.Lax"/> matches the Blazor apps. MVC apps behind a reverse proxy use
    /// <see cref="SameSiteMode.None"/>, because Keycloak's cross-site POST callback would otherwise
    /// drop the correlation cookie. The same value is applied to the nonce cookie.
    /// </summary>
    public SameSiteMode CorrelationCookieSameSite { get; set; } = SameSiteMode.Lax;

    /// <summary>
    /// Secure policy applied to the correlation cookie and the nonce cookie. Defaults to
    /// <see cref="CookieSecurePolicy.SameAsRequest"/>.
    /// </summary>
    public CookieSecurePolicy CorrelationCookieSecurePolicy { get; set; } = CookieSecurePolicy.SameAsRequest;

    /// <summary>
    /// When discovery was fetched through the back-channel, send the browser to the public
    /// authority instead of the internal host that discovery returned. Has no effect when
    /// <see cref="BackchannelBaseUrl"/> is <see langword="null"/> or blank.
    /// </summary>
    public bool RedirectBrowserToPublicAuthority { get; set; }

    /// <summary>
    /// Runs after realm roles and the name identifier have been copied onto the principal.
    /// Used to provision a local user row.
    /// </summary>
    public Func<TokenValidatedContext, Task>? OnTokenValidated { get; set; }
}