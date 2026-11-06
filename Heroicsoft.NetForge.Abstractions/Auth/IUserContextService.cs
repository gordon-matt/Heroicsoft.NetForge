namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Who is making the current request, independent of which auth provider is active.</summary>
public interface IUserContextService
{
    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    /// <returns>The current user identifier.</returns>
    string? GetCurrentUserId();

    /// <summary>
    /// Gets the current user name.
    /// </summary>
    /// <returns>The current user name.</returns>
    string? GetCurrentUserName();

    /// <summary>
    /// Determines whether this instance is authenticated.
    /// </summary>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Determines whether this instance is administrator.
    /// </summary>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    bool IsAdministrator();

    /// <summary>
    /// Gets the role names.
    /// </summary>
    /// <returns>The role names.</returns>
    IReadOnlyList<string> GetRoleNames();
}