using System.Security.Claims;
using Heroicsoft.NetForge.Components.Navigation;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components;

/// <summary>
/// Provides extension methods for net forge menu HTMLs.
/// </summary>
public static class NetForgeMenuHtmlExtensions
{
    /// <summary>The unnamed menu, in the placement it was built with.</summary>
    public static IHtmlContent NetForgeMenu(this IHtmlHelper html) =>
        html.NetForgeMenu(name: null, placement: null);

    /// <summary>The menu for <paramref name="name"/>. An unknown name renders nothing.</summary>
    public static IHtmlContent NetForgeMenu(this IHtmlHelper html, string? name) =>
        html.NetForgeMenu(name, placement: null);

    /// <summary>
    /// The menu for <paramref name="name"/>. <paramref name="placement"/> overrides the placement
    /// chosen when the menu was built.
    /// </summary>
    public static IHtmlContent NetForgeMenu(
        this IHtmlHelper html,
        string? name,
        NetForgeMenuPlacement? placement)
    {
        ArgumentNullException.ThrowIfNull(html);

        var catalog = html.ViewContext.HttpContext.RequestServices.GetService<INetForgeMenuCatalog>();

        if (catalog is null)
        {
            return HtmlString.Empty;
        }

        var http = html.ViewContext.HttpContext;
        string path = $"{http.Request.PathBase}{http.Request.Path}{http.Request.QueryString}";
        var user = http.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        var main = http.RequestServices.GetService<IOptions<ForgeUiOptions>>()?.Value.ActiveSharedMenuPlacement
            ?? NetForgeMenuPlacement.Topbar;
        return NetForgeMenuHtml.Render(catalog.Get(name), user, path, placement, main);
    }
}