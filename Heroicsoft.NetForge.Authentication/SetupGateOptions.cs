namespace Heroicsoft.NetForge.Authentication;

/// <summary>Paths that must keep working while the first-run wizard is incomplete.</summary>
public sealed class SetupGateOptions
{
    public string SetupPath { get; set; } = "/setup";

    /// <summary>
    /// Prefixes that must keep working during setup: the wizard itself, the framework plumbing
    /// Blazor needs to render it, static assets, sign-in, and the health endpoint. Add app
    /// specific paths (an OPDS feed, for example) rather than widening this by default.
    /// </summary>
    public List<string> ExemptPrefixes { get; set; } =
    [
        "/setup",
        "/_blazor",
        "/_framework",
        "/_content",
        "/_vs",
        "/css",
        "/js",
        "/lib",
        "/img",
        "/favicon",
        "/health",
        "/error",
        "/not-found",
        "/access-denied",
        "/auth",
        "/signin-oidc",
        "/signout-callback-oidc",
        "/Identity",
        "/logvue",
        "/hangfire",
    ];
}