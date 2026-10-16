namespace Heroicsoft.NetForge.Authentication;

/// <summary>Path matching for <see cref="SetupRedirectMiddleware"/>, split out so it can be tested.</summary>
public static class SetupPathRules
{
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
    /// A redirect would turn an API call into a confusing 200 with HTML in it, so only browser
    /// navigations are redirected. Everything else gets 503.
    /// </summary>
    public static bool IsNavigation(string method, IEnumerable<string?> acceptValues) =>
        HttpMethods.IsGet(method)
        && acceptValues.Any(value =>
            value?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true);
}