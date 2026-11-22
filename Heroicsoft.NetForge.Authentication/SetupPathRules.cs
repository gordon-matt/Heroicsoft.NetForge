namespace Heroicsoft.NetForge.Authentication;

/// <summary>Path matching for <see cref="SetupRedirectMiddleware"/>, split out so it can be tested.</summary>
public static class SetupPathRules
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="path"/> starts with any entry in
    /// <paramref name="prefixes"/>, ignoring case. A path without a leading slash is also tested
    /// with one added. Returns <see langword="false"/> when <paramref name="path"/> is
    /// <see langword="null"/> or empty.
    /// </summary>
    /// <param name="path">Request path to test.</param>
    /// <param name="prefixes">Exempt prefixes, such as <c>/setup</c> and <c>/auth</c>.</param>
    /// <returns><see langword="true"/> when the path is exempt from the setup redirect.</returns>
    public static bool IsExempt(string? path, IReadOnlyList<string> prefixes)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        var pathString = new PathString(path.StartsWith('/') ? path : "/" + path);

        foreach (string prefix in prefixes)
        {
            if (pathString.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="method"/> is GET and some value in
    /// <paramref name="acceptValues"/> contains <c>text/html</c>. A redirect would turn an API call
    /// into a confusing 200 with HTML in it, so only browser navigations are redirected. Everything
    /// else gets 503.
    /// </summary>
    /// <param name="method">HTTP method of the request.</param>
    /// <param name="acceptValues">Values of the Accept header.</param>
    /// <returns><see langword="true"/> when the middleware should redirect instead of returning 503.</returns>
    public static bool IsNavigation(string method, IEnumerable<string?> acceptValues) =>
        HttpMethods.IsGet(method)
        && acceptValues.Any(value =>
            value?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true);
}