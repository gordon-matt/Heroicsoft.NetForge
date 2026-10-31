using System.Security.Claims;
using Heroicsoft.NetForge.Components.Navigation;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components;

public static class NetForgeMenuHtmlExtensions
{
    /// <summary>The unnamed menu, in the placement it was built with.</summary>
    public static IHtmlContent NetForgeMenu(this IHtmlHelper html) =>
        html.NetForgeMenu(area: null, placement: null);

    /// <summary>The menu for <paramref name="area"/>. An unknown area renders nothing.</summary>
    public static IHtmlContent NetForgeMenu(this IHtmlHelper html, string? area) =>
        html.NetForgeMenu(area, placement: null);

    /// <summary>
    /// The menu for <paramref name="area"/>. <paramref name="placement"/> overrides the placement
    /// chosen when the menu was built.
    /// </summary>
    public static IHtmlContent NetForgeMenu(
        this IHtmlHelper html,
        string? area,
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
        ClaimsPrincipal user = http.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        NetForgeMenuPlacement main = http.RequestServices.GetService<IOptions<ForgeUiOptions>>()?.Value.MainMenuPlacement
            ?? NetForgeMenuPlacement.Topbar;
        return NetForgeMenuHtml.Render(catalog.Get(area), user, path, placement, main);
    }
}
