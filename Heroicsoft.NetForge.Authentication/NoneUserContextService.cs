using Heroicsoft.NetForge.Abstractions.Auth;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// The single synthetic user of <see cref="AuthProvider.None"/>. Always an administrator: with
/// nobody to authenticate there is nobody to withhold anything from, and a mode that locked the
/// only user out of the admin pages would be useless.
/// </summary>
public sealed class NoneUserContextService(IOptions<NetForgeAuthOptions> forgeOptions) : IUserContextService
{
    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    /// <returns>The current user identifier.</returns>
    public string? GetCurrentUserId() => forgeOptions.Value.None.UserId;

    /// <summary>
    /// Gets the current user name.
    /// </summary>
    /// <returns>The current user name.</returns>
    public string? GetCurrentUserName() => forgeOptions.Value.None.UserName;

    /// <summary>
    /// Determines whether this instance is authenticated.
    /// </summary>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    public bool IsAuthenticated() => true;

    /// <summary>
    /// Determines whether this instance is administrator.
    /// </summary>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    public bool IsAdministrator() => true;

    /// <summary>
    /// Gets the role names.
    /// </summary>
    /// <returns>The role names.</returns>
    public IReadOnlyList<string> GetRoleNames() =>
        [forgeOptions.Value.AdministratorRole, forgeOptions.Value.UserRole];
}