using Heroicsoft.NetForge.Abstractions.Auth;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// The single synthetic user of <see cref="AuthProvider.None"/>. Always an administrator: with
/// nobody to authenticate there is nobody to withhold anything from, and a mode that locked the
/// only user out of the admin pages would be useless.
/// </summary>
public sealed class NoneUserContextService(IOptions<NetForgeAuthOptions> forgeOptions) : IUserContextService
{
    public string? GetCurrentUserId() => forgeOptions.Value.None.UserId;

    public string? GetCurrentUserName() => forgeOptions.Value.None.UserName;

    public bool IsAuthenticated() => true;

    public bool IsAdministrator() => true;

    public IReadOnlyList<string> GetRoleNames() =>
        [forgeOptions.Value.AdministratorRole, forgeOptions.Value.UserRole];
}