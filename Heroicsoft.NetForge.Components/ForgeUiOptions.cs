using Heroicsoft.NetForge.Abstractions.Theming;

namespace Heroicsoft.NetForge.Components;

/// <summary>Per-app knobs for the shell. Registered by <c>AddNetForgeComponents</c>.</summary>
public sealed class ForgeUiOptions
{
    /// <summary>Browser <c>localStorage</c> key for the Bootswatch theme. Keep it unique per app.</summary>
    public string ThemeStorageKey { get; set; } = "netforge.theme";

    public string DefaultTheme { get; set; } = BootswatchThemes.Default;

    /// <summary>Format string whose <c>{0}</c> is the theme name. The host serves the CSS.</summary>
    public string StylesheetFormat { get; set; } = "lib/bootswatch/dist/{0}/bootstrap.min.css";

    public string BootstrapScriptPath { get; set; } = "lib/bootstrap/dist/js/bootstrap.bundle.min.js";

    public string IconsStylesheetPath { get; set; } = "lib/bootstrap-icons/font/bootstrap-icons.min.css";
}