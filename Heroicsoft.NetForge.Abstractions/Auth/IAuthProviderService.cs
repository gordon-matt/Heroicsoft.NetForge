namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>
/// Exposes the configured auth mode to the UI so it can hide what doesn't apply — no sign-in
/// menu in <see cref="AuthProvider.None"/> mode, no local password fields under Keycloak.
/// </summary>
public interface IAuthProviderService
{
    /// <summary>Auth mode the host was started with.</summary>
    AuthProvider Provider { get; }

    /// <summary>
    /// Keycloak authority URL, or <see langword="null"/> when the host did not configure one.
    /// </summary>
    string? KeycloakAuthority { get; }

    /// <summary><see langword="true"/> when <see cref="Provider"/> is <see cref="AuthProvider.Keycloak"/>.</summary>
    bool IsKeycloak { get; }

    /// <summary><see langword="true"/> when <see cref="Provider"/> is <see cref="AuthProvider.Identity"/>.</summary>
    bool IsIdentity { get; }

    /// <summary><see langword="true"/> when <see cref="Provider"/> is <see cref="AuthProvider.None"/>.</summary>
    bool IsNone { get; }

    /// <summary>Keycloak account console, or <see langword="null"/> when Keycloak is not the active provider.</summary>
    string? KeycloakAccountUrl { get; }
}