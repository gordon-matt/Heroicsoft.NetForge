namespace Heroicsoft.NetForge.Abstractions.Theming;

/// <summary>
/// Bootswatch themes the shell knows how to swap at runtime. The host still has to restore the
/// matching CSS (see <c>templates/libman.json</c>) — a name listed here with no file behind it
/// 404s when someone picks it.
/// </summary>
public static class BootswatchThemes
{
    /// <summary>Theme used when a stored name is missing or unknown. The value is <c>flatly</c>.</summary>
    public const string Default = "flatly";

    /// <summary>Bootswatch theme names the shell can select, in display order.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        "brite", "cerulean", "cosmo", "cyborg", "darkly", "flatly", "journal", "litera", "lumen",
        "lux", "materia", "minty", "morph", "pulse", "quartz", "sandstone", "simplex", "sketchy",
        "slate", "solar", "spacelab", "superhero", "united", "vapor", "yeti", "zephyr",
    ];

    /// <summary>
    /// Themes with a dark chrome by default. Bootswatch still keys off <c>data-bs-theme</c> to
    /// apply the <c>[data-bs-theme=dark]</c> variable overrides; without it,
    /// <c>--bs-emphasis-color</c> and <c>--bs-tertiary-bg</c> keep their light-theme values.
    /// </summary>
    public static readonly IReadOnlySet<string> DarkColorScheme =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cyborg", "darkly", "slate", "solar", "superhero", "vapor",
        };

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="theme"/> is in <see cref="DarkColorScheme"/>.
    /// The comparison ignores case. Returns <see langword="false"/> when <paramref name="theme"/> is
    /// <see langword="null"/> or empty.
    /// </summary>
    /// <param name="theme">Theme name to test.</param>
    /// <returns><see langword="true"/> when the theme uses a dark color scheme.</returns>
    public static bool IsDarkColorScheme(string? theme) =>
        !string.IsNullOrEmpty(theme) && DarkColorScheme.Contains(theme);

    /// <summary>Names from <see cref="All"/> that are in <see cref="DarkColorScheme"/>, in the same order.</summary>
    public static IEnumerable<string> DarkThemes => All.Where(DarkColorScheme.Contains);

    /// <summary>Names from <see cref="All"/> that are outside <see cref="DarkColorScheme"/>, in the same order.</summary>
    public static IEnumerable<string> LightThemes => All.Where(theme => !DarkColorScheme.Contains(theme));

    /// <summary>
    /// Accepts a stored theme name. Returns <see langword="false"/> when <paramref name="raw"/> is
    /// missing or not in <see cref="All"/>, and sets <paramref name="normalized"/> to
    /// <paramref name="fallback"/> lowercased when that fallback is in <see cref="All"/>.
    /// Otherwise <paramref name="normalized"/> is <see cref="Default"/>. On success,
    /// <paramref name="normalized"/> is <paramref name="raw"/> trimmed and lowercased.
    /// </summary>
    /// <param name="raw">Stored theme name. <see langword="null"/> or whitespace is rejected.</param>
    /// <param name="normalized">Accepted theme, or the fallback described above.</param>
    /// <param name="fallback">Name to use when <paramref name="raw"/> is rejected. <see langword="null"/> or an unknown name selects <see cref="Default"/>.</param>
    /// <returns><see langword="true"/> when <paramref name="raw"/> is a known theme.</returns>
    public static bool TryNormalize(string? raw, out string normalized, string? fallback = null)
    {
        normalized = fallback is not null && All.Contains(fallback, StringComparer.OrdinalIgnoreCase)
            ? fallback.ToLowerInvariant()
            : Default;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        string theme = raw.Trim().ToLowerInvariant();
        if (!All.Contains(theme, StringComparer.Ordinal))
        {
            return false;
        }

        normalized = theme;
        return true;
    }

    /// <summary>
    /// Substitutes <paramref name="theme"/> for <c>{0}</c> in <paramref name="format"/>, using the
    /// invariant culture. The default format is <c>lib/bootswatch/dist/{0}/bootstrap.min.css</c>.
    /// </summary>
    /// <param name="theme">Theme name inserted into the path.</param>
    /// <param name="format">Composite format whose <c>{0}</c> placeholder is the theme name.</param>
    /// <returns>The stylesheet path.</returns>
    public static string StylesheetHref(string theme, string format = "lib/bootswatch/dist/{0}/bootstrap.min.css") =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture, format, theme);
}