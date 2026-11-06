namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Which sign-in system the host was started with.</summary>
public enum AuthProvider
{
    /// <summary>
    /// The identity value.
    /// </summary>
    Identity = 0,
    /// <summary>
    /// The keycloak value.
    /// </summary>
    Keycloak = 1,
    /// <summary>
    /// The none value.
    /// </summary>
    None = 2,
}