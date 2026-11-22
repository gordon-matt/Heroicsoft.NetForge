using Heroicsoft.NetForge.Abstractions.Auth;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// The single synthetic user of <see cref="AuthProvider.None"/>. Always an administrator: with
/// nobody to authenticate there is nobody to withhold anything from, and a mode that locked the
/// only user out of the admin pages would be useless.
/// </summary>
/// <param name="forgeOptions">Supplies the synthetic user id, display name, and role names.</param>
public sealed class NoneUserContextService(IOptions<NetForgeAuthOptions> forgeOptions) : IUserContextService
{
    /// <summary>Returns <see cref="NoneAuthOptions.UserId"/> from the authentication options.</summary>
    /// <returns>The configured synthetic user id.</returns>
    public string? GetCurrentUserId() => forgeOptions.Value.None.UserId;

    /// <summary>Returns <see cref="NoneAuthOptions.UserName"/> from the authentication options.</summary>
    /// <returns>The configured synthetic display name.</returns>
    public string? GetCurrentUserName() => forgeOptions.Value.None.UserName;

    /// <summary>Returns <see langword="true"/>. Nobody signs in, so the synthetic user is always authenticated.</summary>
    /// <returns><see langword="true"/>.</returns>
    public bool IsAuthenticated() => true;

    /// <summary>Returns <see langword="true"/>. The synthetic user is always an administrator.</summary>
    /// <returns><see langword="true"/>.</returns>
    public bool IsAdministrator() => true;

    /// <summary>
    /// Returns <see cref="NetForgeAuthOptions.AdministratorRole"/> and <see cref="NetForgeAuthOptions.UserRole"/>,
    /// in that order.
    /// </summary>
    /// <returns>The two configured role names.</returns>
    public IReadOnlyList<string> GetRoleNames() =>
        [forgeOptions.Value.AdministratorRole, forgeOptions.Value.UserRole];
}