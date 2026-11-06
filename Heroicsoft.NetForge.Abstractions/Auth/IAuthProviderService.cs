namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>
/// Exposes the configured auth mode to the UI so it can hide what doesn't apply — no sign-in
/// menu in <see cref="AuthProvider.None"/> mode, no local password fields under Keycloak.
/// </summary>
public interface IAuthProviderService
{
    /// <summary>
    /// Gets the provider.
    /// </summary>
    AuthProvider Provider { get; }

    /// <summary>
    /// Gets the keycloak authority.
    /// </summary>
    string? KeycloakAuthority { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is keycloak.
    /// </summary>
    bool IsKeycloak { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is identity.
    /// </summary>
    bool IsIdentity { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is none.
    /// </summary>
    bool IsNone { get; }

    /// <summary>Keycloak account console, or null when Keycloak is not the active provider.</summary>
    string? KeycloakAccountUrl { get; }
}