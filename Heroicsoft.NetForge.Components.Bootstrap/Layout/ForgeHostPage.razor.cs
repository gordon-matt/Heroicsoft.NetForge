using Heroicsoft.NetForge.Abstractions.Theming;
using Heroicsoft.NetForge.Components.Bootstrap.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

public partial class ForgeHostPage
{
    [Inject]
    private IOptions<ForgeBootstrapOptions> UiOptions { get; set; } = default!;

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? IconHref { get; set; }

    /// <summary>App stylesheet href, usually <c>@Assets["css/site.css"]</c> from the host.</summary>
    [Parameter]
    public string? AppStylesheet { get; set; }

    [Parameter]
    public IComponentRenderMode? PageRenderMode { get; set; }

    [Parameter]
    public RenderFragment? AdditionalHead { get; set; }

    [Parameter]
    public RenderFragment? AdditionalBody { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Pins this document to an area. Null follows the route, or the first path segment when that
    /// name is listed in <see cref="ForgeBootstrapOptions.Areas"/>. An empty string is the public site.
    /// Set this when a library is mounted somewhere other than <c>/{area}</c>, for example
    /// IdentityNexus at <c>/identity</c> while it should use the Admin theme.
    /// </summary>
    [Parameter]
    public string? Area { get; set; }

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    /// <summary>
    /// Where the static web assets of this package are served. The path has to be spelled out:
    /// inside a class library <c>@Assets["js/forge.js"]</c> resolves against the host, not here.
    /// </summary>
    private const string AssetBase = "_content/Heroicsoft.NetForge.Components.Bootstrap";

    private ForgeBootstrapOptions Ui => UiOptions.Value;

    private string ResolvedArea => ThemeStorage.ResolveArea(
        Area,
        HttpContext?.GetRouteValue("area") as string,
        HttpContext?.Request.Path.Value,
        Ui.Areas.Keys);

    private string StorageKey => ThemeStorage.KeyFor(Ui.ThemeStorageKey, ResolvedArea);

    /// <summary>Null leaves the attribute off the script, so the script follows the URL.</summary>
    private string? PinnedArea => Area is null ? null : ThemeStorage.NormalizeArea(Area);

    private string? AreasAttribute => ThemeStorage.AreasAttribute(Ui.Areas);

    private string InitialTheme
    {
        get
        {
            string configured = ThemeStorage.ValueFor(ResolvedArea, Ui.Areas, Ui.DefaultTheme);
            return BootswatchThemes.TryNormalize(configured, out string theme, BootswatchThemes.Default)
                ? theme
                : BootswatchThemes.Default;
        }
    }

    private string DefaultThemeName =>
        BootswatchThemes.TryNormalize(Ui.DefaultTheme, out string theme, BootswatchThemes.Default)
            ? theme
            : BootswatchThemes.Default;

    private string InitialScheme => BootswatchThemes.IsDarkColorScheme(InitialTheme) ? "dark" : "light";

    private IComponentRenderMode RenderModeInUse =>
        PageRenderMode ?? Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveServer;
}