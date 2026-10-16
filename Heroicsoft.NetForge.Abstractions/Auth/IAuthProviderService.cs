namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>
/// Exposes the configured auth mode to the UI so it can hide what doesn't apply — no sign-in
/// menu in <see cref="AuthProvider.None"/> mode, no local password fields under Keycloak.
/// </summary>
public interface IAuthProviderService
{
    AuthProvider Provider { get; }

    string? KeycloakAuthority { get; }

    bool IsKeycloak { get; }

    bool IsIdentity { get; }

    bool IsNone { get; }

    /// <summary>Keycloak account console, or null when Keycloak is not the active provider.</summary>
    string? KeycloakAccountUrl { get; }
}