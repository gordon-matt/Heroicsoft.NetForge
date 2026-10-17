namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>Decides whether a sidebar link is the current page, including its query string.</summary>
public static class ForgeNavMatch
{
    public static bool IsCurrent(ForgeNavItem item, string relativeUrl)
    {
        var (path, query) = Split(relativeUrl);

        return PathMatches(item, path) && QueryMatches(item.ActiveQuery, query);
    }

    private static bool PathMatches(ForgeNavItem item, string currentPath)
    {
        currentPath = currentPath.Trim('/');
        IEnumerable<string> candidates = item.ActivePaths is { Count: > 0 }
            ? item.ActivePaths
            : [PathOf(item.Href)];

        foreach (string candidate in candidates)
        {
            string expected = PathOf(candidate);

            if (item.MatchAll)
            {
                if (string.Equals(expected, currentPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (expected.Length == 0
                || string.Equals(expected, currentPath, StringComparison.OrdinalIgnoreCase)
                || currentPath.StartsWith(expected + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool QueryMatches(IReadOnlyDictionary<string, string?>? expected, IReadOnlyDictionary<string, string> actual)
    {
        if (expected is null)
        {
            return true;
        }

        foreach (var (key, value) in expected)
        {
            actual.TryGetValue(key, out string? found);

            if (value is null)
            {
                if (!string.IsNullOrWhiteSpace(found))
                {
                    return false;
                }
            }
            else if (!string.Equals(found, value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static (string Path, Dictionary<string, string> Query) Split(string relativeUrl)
    {
        string withoutHash = relativeUrl.Split('#')[0];
        int queryIndex = withoutHash.IndexOf('?');
        string path = queryIndex < 0 ? withoutHash : withoutHash[..queryIndex];
        string query = queryIndex < 0 ? string.Empty : withoutHash[queryIndex..];

        return (path, ForgeQuery.Parse(query));
    }

    private static string PathOf(string href) => href.Split('?', '#')[0].Trim('/');
}
