namespace Heroicsoft.NetForge.Abstractions.Theming;

/// <summary>
/// Bootswatch themes the shell knows how to swap at runtime. The host still has to restore the
/// matching CSS (see <c>templates/libman.json</c>) — a name listed here with no file behind it
/// 404s when someone picks it.
/// </summary>
public static class BootswatchThemes
{
    public const string Default = "flatly";

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

    public static bool IsDarkColorScheme(string? theme) =>
        !string.IsNullOrEmpty(theme) && DarkColorScheme.Contains(theme);

    public static IEnumerable<string> DarkThemes => All.Where(DarkColorScheme.Contains);

    public static IEnumerable<string> LightThemes => All.Where(theme => !DarkColorScheme.Contains(theme));

    /// <summary>
    /// Accepts a stored theme name. Returns false when <paramref name="raw"/> is missing or not
    /// in <see cref="All"/>, and leaves <paramref name="normalized"/> as the fallback.
    /// </summary>
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

    public static string StylesheetHref(string theme, string format = "lib/bootswatch/dist/{0}/bootstrap.min.css") =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture, format, theme);
}