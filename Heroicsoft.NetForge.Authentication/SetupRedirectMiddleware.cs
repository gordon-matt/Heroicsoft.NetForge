using Heroicsoft.NetForge.Abstractions.Setup;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Sends every request to the setup wizard until <see cref="ISetupStatus"/> reports complete.
/// </summary>
/// <remarks>
/// Place it after authentication so the wizard can tell the visitor who they are signed in as,
/// and before endpoint routing so it can short-circuit Blazor and controller routes alike. The
/// exemption list is deliberately narrow: anything not needed to render and complete the wizard
/// stays unreachable, which keeps an unconfigured instance from exposing half-initialised state.
/// </remarks>
public sealed class SetupRedirectMiddleware(
    RequestDelegate next,
    IOptions<SetupGateOptions> options)
{
    public async Task InvokeAsync(HttpContext context, ISetupStatus setupStatus)
    {
        var gate = options.Value;

        if (SetupPathRules.IsExempt(context.Request.Path.Value, gate.ExemptPrefixes))
        {
            await next(context);
            return;
        }

        if (await setupStatus.IsCompleteAsync(context.RequestAborted))
        {
            await next(context);
            return;
        }

        if (!SetupPathRules.IsNavigation(context.Request.Method, context.Request.Headers.Accept))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        context.Response.Redirect(gate.SetupPath);
    }
}