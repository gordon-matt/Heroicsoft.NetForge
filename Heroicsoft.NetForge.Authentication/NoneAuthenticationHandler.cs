using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Authenticates every request as a single synthetic administrator.
/// </summary>
/// <remarks>
/// For running an app on your own machine, where being made to invent a password first is pure
/// friction. There is no authentication here at all, by design, so this mode must never be
/// selected for an instance anyone else can reach.
/// </remarks>
/// <param name="options">Scheme options passed to the base handler.</param>
/// <param name="logger">Logger passed to the base handler.</param>
/// <param name="encoder">URL encoder passed to the base handler.</param>
/// <param name="forgeOptions">Supplies the synthetic user's id, name, and roles.</param>
public sealed class NoneAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<NetForgeAuthOptions> forgeOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>Authentication scheme name <c>None</c>.</summary>
    public const string SchemeName = "None";

    /// <summary>
    /// Authenticates the request as the synthetic user from <see cref="NetForgeAuthOptions.None"/>,
    /// with <see cref="NetForgeAuthOptions.AdministratorRole"/> and <see cref="NetForgeAuthOptions.UserRole"/>.
    /// The result is always success.
    /// </summary>
    /// <returns>A successful ticket for the synthetic administrator.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configured = forgeOptions.Value;

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, configured.None.UserId),
                new Claim(ClaimTypes.Name, configured.None.UserName),
                new Claim(ClaimTypes.Role, configured.AdministratorRole),
                new Claim(ClaimTypes.Role, configured.UserRole),
            ],
            authenticationType: SchemeName,
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}