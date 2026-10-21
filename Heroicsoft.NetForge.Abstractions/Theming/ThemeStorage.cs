using System.Text.Json;

namespace Heroicsoft.NetForge.Abstractions.Theming;

/// <summary>
/// The browser <c>localStorage</c> key for a theme, shared by every NetForge screen in one area.
/// The default area (the public site, an empty name) keeps <see cref="DefaultKey"/> or the app's
/// base key unchanged. A named area such as Admin appends a lower-case suffix, so the public site
/// and the back office can remember different choices. LogVue, IdentityNexus, and the shell all
/// call <see cref="KeyFor"/> so a change in one of them updates the others.
/// </summary>
public static class ThemeStorage
{
    /// <summary>Key used when an app does not set its own. Existing apps pass their own base key.</summary>
    public const string DefaultKey = "netforge.theme";

    /// <summary>
    /// <paramref name="baseKey"/> for the default area, or <c>{baseKey}.{area}</c> for a named area.
    /// "Admin" and "admin" are the same area. A missing base key becomes <see cref="DefaultKey"/>.
    /// </summary>
    public static string KeyFor(string? baseKey, string? area)
    {
        string key = string.IsNullOrWhiteSpace(baseKey) ? DefaultKey : baseKey.Trim();
        string name = NormalizeArea(area);
        return name.Length == 0 ? key : key + "." + name;
    }

    /// <summary>Empty string for the default area. Otherwise the trimmed, lower-case area name.</summary>
    public static string NormalizeArea(string? area) =>
        string.IsNullOrWhiteSpace(area) ? "" : area.Trim().ToLowerInvariant();

    /// <summary>
    /// Which area this request belongs to. <paramref name="explicitArea"/> wins, and an explicit
    /// empty string pins the page to the default area. Otherwise a route value or the first path
    /// segment is used when it is one of <paramref name="namedAreas"/>. An area that is not listed,
    /// including Identity, stays on the default area so it shares that theme.
    /// </summary>
    public static string ResolveArea(
        string? explicitArea,
        string? routeArea,
        string? requestPath,
        IEnumerable<string>? namedAreas)
    {
        if (explicitArea is not null)
        {
            return NormalizeArea(explicitArea);
        }

        var known = KnownAreas(namedAreas);

        string route = NormalizeArea(routeArea);
        if (route.Length > 0 && known.Contains(route))
        {
            return route;
        }

        string segment = FirstSegment(requestPath);
        if (segment.Length > 0 && known.Contains(segment))
        {
            return segment;
        }

        return "";
    }

    /// <summary>
    /// The configured default for <paramref name="area"/>, or <paramref name="fallback"/> when that
    /// area has none. Used for the colour scheme (light, dark, system) and for a Bootswatch name.
    /// </summary>
    public static string ValueFor(string? area, IReadOnlyDictionary<string, string?>? areas, string fallback)
    {
        string name = NormalizeArea(area);
        if (name.Length > 0 && areas is not null)
        {
            foreach (var pair in areas)
            {
                if (NormalizeArea(pair.Key) == name && !string.IsNullOrWhiteSpace(pair.Value))
                {
                    return pair.Value.Trim();
                }
            }
        }

        return fallback;
    }

    /// <summary>
    /// JSON object of area name to default, for the <c>data-theme-areas</c> attribute on
    /// <c>theme-init.js</c>. Null when there is nothing to advertise, so a page with no named
    /// areas keeps today's single key. An area with a null default is included with an empty
    /// value: it still has its own key, and the script falls back to the app default.
    /// </summary>
    public static string? AreasAttribute(IReadOnlyDictionary<string, string?>? areas)
    {
        if (areas is null || areas.Count == 0)
        {
            return null;
        }

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in areas)
        {
            string name = NormalizeArea(pair.Key);
            if (name.Length == 0)
            {
                continue;
            }

            map[name] = string.IsNullOrWhiteSpace(pair.Value) ? "" : pair.Value.Trim().ToLowerInvariant();
        }

        return map.Count == 0 ? null : JsonSerializer.Serialize(map);
    }

    private static HashSet<string> KnownAreas(IEnumerable<string>? namedAreas)
    {
        var known = new HashSet<string>(StringComparer.Ordinal);
        if (namedAreas is null)
        {
            return known;
        }

        foreach (string name in namedAreas)
        {
            string normalized = NormalizeArea(name);
            if (normalized.Length > 0)
            {
                known.Add(normalized);
            }
        }

        return known;
    }

    private static string FirstSegment(string? requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath))
        {
            return "";
        }

        string path = requestPath.Trim();
        int cut = path.IndexOfAny(['?', '#']);
        if (cut >= 0)
        {
            path = path[..cut];
        }

        if (path.StartsWith('/'))
        {
            path = path[1..];
        }

        int slash = path.IndexOf('/');
        if (slash >= 0)
        {
            path = path[..slash];
        }

        return NormalizeArea(path);
    }
}
