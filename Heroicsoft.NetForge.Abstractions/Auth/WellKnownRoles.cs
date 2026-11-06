namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>
/// Role names the shell and the Hangfire dashboard already check for. Apps can use different
/// names by setting <c>Authentication:AdministratorRole</c>, but these are the defaults every
/// existing app ships with.
/// </summary>
public static class WellKnownRoles
{
    /// <summary>
    /// The administrator constant.
    /// </summary>
    public const string Administrator = "Administrator";

    /// <summary>
    /// The user constant.
    /// </summary>
    public const string User = "User";
}