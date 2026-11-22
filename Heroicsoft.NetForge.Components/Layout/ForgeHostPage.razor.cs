using Heroicsoft.NetForge.Abstractions.Theming;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders the HTML document: the theme script, the NetForge stylesheet, an optional accent and app stylesheet, and the Blazor host.
/// </summary>
public partial class ForgeHostPage
{
    [Inject]
    private IOptions<ForgeUiOptions> UiOptions { get; set; } = default!;

    /// <summary>
    /// Document title. Required. The default is empty.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Favicon address. <see langword="null"/> or whitespace omits the icon link.
    /// </summary>
    [Parameter]
    public string? IconHref { get; set; }

    /// <summary>App stylesheet href, usually <c>@Assets["css/site.css"]</c> from the host.</summary>
    [Parameter]
    public string? AppStylesheet { get; set; }

    /// <summary>
    /// Render mode of the head outlet. <see langword="null"/> uses interactive server.
    /// </summary>
    [Parameter]
    public IComponentRenderMode? PageRenderMode { get; set; }

    /// <summary>
    /// Extra head content, after the stylesheets. <see langword="null"/> renders nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? AdditionalHead { get; set; }

    /// <summary>
    /// Extra content after the NetForge script. <see langword="null"/> renders nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? AdditionalBody { get; set; }

    /// <summary>
    /// The document body, before the Blazor and NetForge scripts. <see langword="null"/> leaves the body to those scripts.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Pins this document to an area. Null follows the route, or the first path segment when that
    /// name is listed in <see cref="ForgeUiOptions.Areas"/>. An empty string is the public site.
    /// Set this when a library is mounted somewhere other than <c>/{area}</c>, for example
    /// IdentityNexus at <c>/identity</c> while it should use the Admin theme.
    /// </summary>
    [Parameter]
    public string? Area { get; set; }

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    /// <summary>
    /// Where the static web assets of this package are served. The path has to be spelled out:
    /// inside a class library <c>@Assets["js/nf.js"]</c> resolves against the host, not here.
    /// </summary>
    private const string AssetBase = "_content/Heroicsoft.NetForge.Components";

    private ForgeUiOptions Ui => UiOptions.Value;

    private string? AccentCss => ForgeAccentCss.Build(Ui);

    private string ResolvedArea => ThemeStorage.ResolveArea(
        Area,
        HttpContext?.GetRouteValue("area") as string,
        HttpContext?.Request.Path.Value,
        Ui.Areas.Keys);

    private string StorageKey => ThemeStorage.KeyFor(Ui.ThemeStorageKey, ResolvedArea);

    /// <summary>Null leaves the attribute off the script, so the script follows the URL.</summary>
    private string? PinnedArea => Area is null ? null : ThemeStorage.NormalizeArea(Area);

    private string? AreasAttribute => ThemeStorage.AreasAttribute(Ui.Areas);

    /// <summary>The scheme server-rendered markup starts in; theme-init.js refines it before paint.</summary>
    private string InitialTheme => SchemeMarkup(ThemeStorage.ValueFor(ResolvedArea, Ui.Areas, Ui.DefaultColorScheme ?? "light"));

    private string DefaultScheme =>
        Ui.DefaultColorScheme?.Trim().ToLowerInvariant() switch
        {
            "dark" => "dark",
            "system" => "system",
            _ => "light",
        };

    private static string SchemeMarkup(string scheme) =>
        string.Equals(scheme, "dark", StringComparison.OrdinalIgnoreCase) ? "dark" : "light";

    private IComponentRenderMode RenderModeInUse =>
        PageRenderMode ?? Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveServer;
}