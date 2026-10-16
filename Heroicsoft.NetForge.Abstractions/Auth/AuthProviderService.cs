namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Immutable snapshot of the auth mode chosen at startup.</summary>
public sealed class AuthProviderService(AuthProvider provider, string? keycloakAuthority) : IAuthProviderService
{
    public AuthProvider Provider { get; } = provider;

    public string? KeycloakAuthority { get; } = keycloakAuthority;

    public bool IsKeycloak => Provider == AuthProvider.Keycloak;

    public bool IsIdentity => Provider == AuthProvider.Identity;

    public bool IsNone => Provider == AuthProvider.None;

    public string? KeycloakAccountUrl =>
        IsKeycloak && !string.IsNullOrWhiteSpace(KeycloakAuthority)
            ? KeycloakAuthority.TrimEnd('/') + "/account"
            : null;
}