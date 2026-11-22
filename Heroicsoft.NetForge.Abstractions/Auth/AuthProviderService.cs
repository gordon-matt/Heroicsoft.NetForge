namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Immutable snapshot of the auth mode chosen at startup.</summary>
/// <param name="provider">Auth mode selected when the host started.</param>
/// <param name="keycloakAuthority">Keycloak authority URL, or <see langword="null"/> when the host did not set one.</param>
public sealed class AuthProviderService(AuthProvider provider, string? keycloakAuthority) : IAuthProviderService
{
    /// <inheritdoc/>
    public AuthProvider Provider { get; } = provider;

    /// <inheritdoc/>
    public string? KeycloakAuthority { get; } = keycloakAuthority;

    /// <inheritdoc/>
    public bool IsKeycloak => Provider == AuthProvider.Keycloak;

    /// <inheritdoc/>
    public bool IsIdentity => Provider == AuthProvider.Identity;

    /// <inheritdoc/>
    public bool IsNone => Provider == AuthProvider.None;

    /// <summary>
    /// Keycloak account console URL: trailing slashes are removed from <see cref="KeycloakAuthority"/>
    /// and <c>/account</c> is appended. <see langword="null"/> when <see cref="IsKeycloak"/> is
    /// <see langword="false"/> or the authority is <see langword="null"/> or blank.
    /// </summary>
    public string? KeycloakAccountUrl =>
        IsKeycloak && !string.IsNullOrWhiteSpace(KeycloakAuthority)
            ? KeycloakAuthority.TrimEnd('/') + "/account"
            : null;
}