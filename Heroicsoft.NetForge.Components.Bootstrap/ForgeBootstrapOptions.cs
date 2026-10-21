using Heroicsoft.NetForge.Abstractions.Theming;

namespace Heroicsoft.NetForge.Components.Bootstrap;

/// <summary>Per-app knobs for the Bootstrap shell. Registered by <c>AddNetForgeBootstrapComponents</c>.</summary>
public sealed class ForgeBootstrapOptions
{
    /// <summary>
    /// Browser <c>localStorage</c> key for the default area (the public site). Named areas append
    /// a suffix; see <see cref="ThemeStorage.KeyFor"/>. Libraries mounted in this app should use
    /// this same base key rather than one of their own, so they follow the shell.
    /// </summary>
    public string ThemeStorageKey { get; set; } = ThemeStorage.DefaultKey;

    public string DefaultTheme { get; set; } = BootswatchThemes.Default;

    /// <summary>
    /// Areas that keep their own saved Bootswatch theme. The key is the area name (<c>Admin</c>).
    /// The value is that area's default theme until the visitor chooses, or null to use
    /// <see cref="DefaultTheme"/>. The default area is not listed. A route or path is only treated
    /// as one of these areas when the name is present here.
    /// </summary>
    public Dictionary<string, string?> Areas { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Format string whose <c>{0}</c> is the theme name. The host serves the CSS.</summary>
    public string StylesheetFormat { get; set; } = "lib/bootswatch/dist/{0}/bootstrap.min.css";

    public string BootstrapScriptPath { get; set; } = "lib/bootstrap/dist/js/bootstrap.bundle.min.js";

    public string IconsStylesheetPath { get; set; } = "lib/bootstrap-icons/font/bootstrap-icons.min.css";
}
