using System.Security.Claims;
using Heroicsoft.NetForge.Components.Navigation;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components;

/// <summary>Renders a shared menu from an MVC view.</summary>
public static class NetForgeMenuHtmlExtensions
{
    /// <summary>The unnamed menu, in the placement it was built with.</summary>
    /// <param name="html">The view helper. The menu is read from its request services.</param>
    /// <returns>The menu markup, or empty markup when the catalog is missing or the menu was not built.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="html"/> is <see langword="null"/>.</exception>
    public static IHtmlContent NetForgeMenu(this IHtmlHelper html) =>
        html.NetForgeMenu(name: null, placement: null);

    /// <summary>The menu for <paramref name="name"/>. An unknown name renders nothing.</summary>
    /// <param name="html">The view helper. The menu is read from its request services.</param>
    /// <param name="name">Menu name. <see langword="null"/> or empty selects the unnamed menu. An unknown name renders nothing.</param>
    /// <returns>The menu markup, or empty markup when there is nothing to show.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="html"/> is <see langword="null"/>.</exception>
    public static IHtmlContent NetForgeMenu(this IHtmlHelper html, string? name) =>
        html.NetForgeMenu(name, placement: null);

    /// <summary>
    /// The menu for <paramref name="name"/>. <paramref name="placement"/> overrides the placement
    /// chosen when the menu was built.
    /// </summary>
    /// <param name="html">The view helper. The menu is read from its request services.</param>
    /// <param name="name">Menu name. <see langword="null"/> or empty selects the unnamed menu. An unknown name renders nothing.</param>
    /// <param name="placement">Overrides the placement chosen when the menu was built. <see langword="null"/> keeps that placement.</param>
    /// <returns>The menu markup, or empty markup when the catalog is missing, the menu was not built, or the chosen slot has no sections.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="html"/> is <see langword="null"/>.</exception>
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