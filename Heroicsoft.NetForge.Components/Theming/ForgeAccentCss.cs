using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Heroicsoft.NetForge.Components.Theming;

/// <summary>
/// Turns <see cref="ForgeUiOptions.AccentColor"/> and <see cref="ForgeUiOptions.AccentColorDark"/>
/// into the CSS that overrides the stylesheet's default indigo. <c>ForgeHostPage</c> emits it in a
/// <c>style</c> element; apps with their own host page can call <see cref="Build"/> directly.
/// </summary>
public static partial class ForgeAccentCss
{
    /// <summary>Text colour used on an accent that is light enough to need dark text.</summary>
    public const string DarkText = "#0d1015";

    /// <summary>Text colour used on an accent that is dark enough to need white text.</summary>
    public const string LightText = "#ffffff";

    /// <summary>
    /// Returns <c>:root{--nf-accent:...}[data-theme=dark]{--nf-accent:...}</c>, or null when no
    /// accent is configured. Values that are not a plain colour are ignored rather than emitted:
    /// the result goes into a style element, so anything that could close it or add rules is refused.
    /// </summary>
    public static string? Build(ForgeUiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        string? light = Sanitize(options.AccentColor);
        string? dark = Sanitize(options.AccentColorDark) ?? light;

        if (light is null && dark is null)
        {
            return null;
        }

        var css = new StringBuilder();

        if (light is not null)
        {
            css.Append(":root{").Append(Declarations(light)).Append('}');
        }

        // Always emitted when anything is set: the stylesheet's own dark rule has the same
        // specificity as :root, so without this the default dark accent would win in dark mode.
        if (dark is not null)
        {
            css.Append("[data-theme=dark]{").Append(Declarations(dark)).Append('}');
        }

        return css.ToString();
    }

    /// <summary>
    /// Picks white or near-black text for a hex accent, whichever reads better on it. Null for
    /// anything that is not a hex colour, which leaves the stylesheet's default text colour alone.
    /// </summary>
    public static string? ContrastText(string? color)
    {
        if (!TryParseHex(color, out double red, out double green, out double blue))
        {
            return null;
        }

        double luminance = (0.2126 * Linear(red)) + (0.7152 * Linear(green)) + (0.0722 * Linear(blue));

        // White wins while its contrast ratio (1.05 / (L + 0.05)) beats black's ((L + 0.05) / 0.05).
        return luminance <= 0.179 ? LightText : DarkText;
    }

    /// <summary>Returns the trimmed colour when it is safe to write into CSS, otherwise null.</summary>
    public static string? Sanitize(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return null;
        }

        string value = color.Trim();

        return value.Length <= 80 && SafeColor().IsMatch(value) ? value : null;
    }

    private static string Declarations(string color) =>
        ContrastText(color) is { } text
            ? $"--nf-accent:{color};--nf-accent-text:{text};"
            : $"--nf-accent:{color};";

    private static bool TryParseHex(string? color, out double red, out double green, out double blue)
    {
        red = green = blue = 0;

        if (string.IsNullOrWhiteSpace(color))
        {
            return false;
        }

        string hex = color.Trim();

        if (hex[0] != '#')
        {
            return false;
        }

        hex = hex[1..];

        if (hex.Length is 3 or 4)
        {
            hex = string.Concat(hex.Select(digit => new string(digit, 2)));
        }

        if (hex.Length is not (6 or 8)
            || !int.TryParse(hex.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r)
            || !int.TryParse(hex.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g)
            || !int.TryParse(hex.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
        {
            return false;
        }

        red = r / 255d;
        green = g / 255d;
        blue = b / 255d;
        return true;
    }

    private static double Linear(double channel) =>
        channel <= 0.04045 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);

    // A hex colour, a colour function with plain numeric arguments, or a named colour.
    [GeneratedRegex(
        @"^(#[0-9a-fA-F]{3,8}|(rgb|rgba|hsl|hsla|hwb|lab|lch|oklab|oklch)\([0-9a-zA-Z.,%\s/+\-]+\)|[a-zA-Z]{3,24})$")]
    private static partial Regex SafeColor();
}