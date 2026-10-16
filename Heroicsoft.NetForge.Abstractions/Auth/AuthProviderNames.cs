namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Values accepted by <c>Authentication:Provider</c>.</summary>
public static class AuthProviderNames
{
    public const string Identity = "Identity";

    public const string Keycloak = "Keycloak";

    public const string None = "None";

    public static readonly IReadOnlyList<string> All = [Identity, Keycloak, None];
}