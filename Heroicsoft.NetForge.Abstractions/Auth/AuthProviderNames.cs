namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Values accepted by <c>Authentication:Provider</c>.</summary>
public static class AuthProviderNames
{
    /// <summary>Configuration value for <see cref="AuthProvider.Identity"/>. The text is <c>Identity</c>.</summary>
    public const string Identity = "Identity";

    /// <summary>Configuration value for <see cref="AuthProvider.Keycloak"/>. The text is <c>Keycloak</c>.</summary>
    public const string Keycloak = "Keycloak";

    /// <summary>Configuration value for <see cref="AuthProvider.None"/>. The text is <c>None</c>.</summary>
    public const string None = "None";

    /// <summary><see cref="Identity"/>, <see cref="Keycloak"/>, and <see cref="None"/>, in that order.</summary>
    public static readonly IReadOnlyList<string> All = [Identity, Keycloak, None];
}