using Heroicsoft.NetForge.Abstractions.Theming;

namespace Heroicsoft.NetForge.Components;

/// <summary>
/// Per-app knobs for the nf-styled shell. Registered by <c>AddNetForgeComponents</c>. The
/// Bootstrap component set has its own options type.
/// </summary>
public sealed class ForgeUiOptions
{
    /// <summary>
    /// Browser <c>localStorage</c> key for the default area (the public site). Named areas append
    /// a suffix; see <see cref="ThemeStorage.KeyFor"/>. Libraries mounted in this app should use
    /// this same base key rather than one of their own, so they follow the shell.
    /// </summary>
    public string ThemeStorageKey { get; set; } = ThemeStorage.DefaultKey;

    /// <summary>
    /// <c>light</c>, <c>dark</c>, or <c>system</c> (follow the operating system). Applied before first
    /// paint when nothing is stored yet for the area being shown.
    /// </summary>
    public string DefaultColorScheme { get; set; } = "light";

    /// <summary>
    /// Areas that keep their own saved scheme. The key is the area name (<c>Admin</c>). The value
    /// is that area's default (<c>light</c>, <c>dark</c>, or <c>system</c>) until the visitor
    /// chooses, or null to use <see cref="DefaultColorScheme"/>. The default area is not listed.
    /// A route or path is only treated as one of these areas when the name is present here.
    /// </summary>
    public Dictionary<string, string?> Areas { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Accent colour for this app (hex, <c>rgb()</c>, <c>hsl()</c>, or a colour name), written to
    /// <c>--nf-accent</c> by the host page. Leave null to use the stylesheet's default indigo.
    /// </summary>
    public string? AccentColor { get; set; }

    /// <summary>Dark-scheme accent. Falls back to <see cref="AccentColor"/> when null.</summary>
    public string? AccentColorDark { get; set; }
}
