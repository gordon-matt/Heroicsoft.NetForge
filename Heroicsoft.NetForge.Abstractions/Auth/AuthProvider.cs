namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Which sign-in system the host was started with.</summary>
public enum AuthProvider
{
    Identity = 0,
    Keycloak = 1,
    None = 2,
}