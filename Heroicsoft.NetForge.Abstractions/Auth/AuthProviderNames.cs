namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Values accepted by <c>Authentication:Provider</c>.</summary>
public static class AuthProviderNames
{
    /// <summary>
    /// The identity constant.
    /// </summary>
    public const string Identity = "Identity";

    /// <summary>
    /// The keycloak constant.
    /// </summary>
    public const string Keycloak = "Keycloak";

    /// <summary>
    /// The none constant.
    /// </summary>
    public const string None = "None";

    /// <summary>
    /// The all.
    /// </summary>
    public static readonly IReadOnlyList<string> All = [Identity, Keycloak, None];
}