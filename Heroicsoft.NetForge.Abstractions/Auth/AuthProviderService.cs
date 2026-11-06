namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Immutable snapshot of the auth mode chosen at startup.</summary>
public sealed class AuthProviderService(AuthProvider provider, string? keycloakAuthority) : IAuthProviderService
{
    /// <summary>
    /// Gets the provider.
    /// </summary>
    public AuthProvider Provider { get; } = provider;

    /// <summary>
    /// Gets the keycloak authority.
    /// </summary>
    public string? KeycloakAuthority { get; } = keycloakAuthority;

    /// <summary>
    /// Gets a value indicating whether this instance is keycloak.
    /// </summary>
    public bool IsKeycloak => Provider == AuthProvider.Keycloak;

    /// <summary>
    /// Gets a value indicating whether this instance is identity.
    /// </summary>
    public bool IsIdentity => Provider == AuthProvider.Identity;

    /// <summary>
    /// Gets a value indicating whether this instance is none.
    /// </summary>
    public bool IsNone => Provider == AuthProvider.None;

    /// <summary>
    /// Gets the keycloak account URL.
    /// </summary>
    public string? KeycloakAccountUrl =>
        IsKeycloak && !string.IsNullOrWhiteSpace(KeycloakAuthority)
            ? KeycloakAuthority.TrimEnd('/') + "/account"
            : null;
}