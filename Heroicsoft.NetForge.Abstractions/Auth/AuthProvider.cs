namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Which sign-in system the host was started with.</summary>
public enum AuthProvider
{
    /// <summary>Local accounts stored by ASP.NET Core Identity.</summary>
    Identity = 0,

    /// <summary>OpenID Connect sign-in against Keycloak, with the session kept in a cookie.</summary>
    Keycloak = 1,

    /// <summary>No sign-in. Requests run as one synthetic administrator.</summary>
    None = 2,
}