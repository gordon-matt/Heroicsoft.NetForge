namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>Who is making the current request, independent of which auth provider is active.</summary>
public interface IUserContextService
{
    /// <summary>Returns the current caller's user id, or <see langword="null"/> when the caller has none.</summary>
    /// <returns>The user id, or <see langword="null"/>.</returns>
    string? GetCurrentUserId();

    /// <summary>Returns the current caller's display name, or <see langword="null"/> when the caller has none.</summary>
    /// <returns>The display name, or <see langword="null"/>.</returns>
    string? GetCurrentUserName();

    /// <summary>Returns <see langword="true"/> when the current caller is signed in.</summary>
    /// <returns><see langword="true"/> when the caller is signed in; otherwise <see langword="false"/>.</returns>
    bool IsAuthenticated();

    /// <summary>Returns <see langword="true"/> when the current caller is an administrator.</summary>
    /// <returns><see langword="true"/> when the caller is an administrator; otherwise <see langword="false"/>.</returns>
    bool IsAdministrator();

    /// <summary>Returns the current caller's role names. The list is empty when the caller has none.</summary>
    /// <returns>Role names for the current caller.</returns>
    IReadOnlyList<string> GetRoleNames();
}