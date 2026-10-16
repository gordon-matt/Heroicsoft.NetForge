namespace Heroicsoft.NetForge.Components;

/// <summary>
/// Per-app knobs for the nf-styled shell. Registered by <c>AddNetForgeComponents</c>. The
/// Bootstrap component set has its own options type.
/// </summary>
public sealed class ForgeUiOptions
{
    /// <summary>Browser <c>localStorage</c> key for the light or dark scheme. Keep it unique per app.</summary>
    public string ThemeStorageKey { get; set; } = "netforge.theme";

    /// <summary>
    /// <c>light</c>, <c>dark</c>, or <c>system</c> (follow the operating system). Applied before first
    /// paint when nothing is stored yet.
    /// </summary>
    public string DefaultColorScheme { get; set; } = "light";

    /// <summary>
    /// Accent colour for this app (hex, <c>rgb()</c>, <c>hsl()</c>, or a colour name), written to
    /// <c>--nf-accent</c> by the host page. Leave null to use the stylesheet's default indigo.
    /// </summary>
    public string? AccentColor { get; set; }

    /// <summary>Dark-scheme accent. Falls back to <see cref="AccentColor"/> when null.</summary>
    public string? AccentColorDark { get; set; }
}
