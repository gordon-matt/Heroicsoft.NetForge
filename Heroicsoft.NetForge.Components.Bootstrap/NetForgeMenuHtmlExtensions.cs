using System.Security.Claims;
using Heroicsoft.NetForge.Components.Bootstrap.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Bootstrap;

/// <summary>
/// Renders a shared menu as Bootstrap HTML from an MVC view.
/// </summary>
public static class NetForgeMenuHtmlExtensions
{
    /// <summary>
    /// Renders the unnamed menu into the app's active shared-menu slot. That slot is the top bar unless the host changed it.
    /// </summary>
    /// <param name="html">The view to render into. The current request supplies the user and the path.</param>
    /// <returns>
    /// Bootstrap navigation markup, or empty HTML when no menu catalog is registered, the menu was never built, or the user can see no sections.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="html"/> is <see langword="null"/>.</exception>
    public static IHtmlContent NetForgeMenu(this IHtmlHelper html) =>
        html.NetForgeMenu(name: null, placement: null);

    /// <summary>Renders the menu for <paramref name="name"/>. An unknown name renders nothing.</summary>
    /// <param name="html">The view to render into. The current request supplies the user and the path.</param>
    /// <param name="name">The menu name. <see langword="null"/> or blank selects the unnamed menu.</param>
    /// <returns>
    /// Bootstrap navigation markup, or empty HTML when no menu catalog is registered, the name is unknown, or the user can see no sections.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="html"/> is <see langword="null"/>.</exception>
    public static IHtmlContent NetForgeMenu(this IHtmlHelper html, string? name) =>
        html.NetForgeMenu(name, placement: null);

    /// <summary>
    /// Renders the menu for <paramref name="name"/> into one slot.
    /// <paramref name="placement"/> chooses the top bar or the sidebar markup.
    /// </summary>
    /// <param name="html">The view to render into. The current request supplies the user and the path.</param>
    /// <param name="name">The menu name. <see langword="null"/> or blank selects the unnamed menu. An unknown name renders nothing.</param>
    /// <param name="placement">
    /// Slot to write. <see langword="null"/> uses the app's active shared-menu placement, which defaults to the top bar.
    /// <see cref="NetForgeMenuPlacement.Topbar"/> writes the collapsible top navigation.
    /// <see cref="NetForgeMenuPlacement.Sidebar"/> writes the vertical navigation.
    /// </param>
    /// <returns>
    /// Bootstrap navigation markup, or empty HTML when no menu catalog is registered, the menu was never built, or no section belongs in the slot.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="html"/> is <see langword="null"/>.</exception>
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
        return BootstrapNetForgeMenuHtml.Render(catalog.Get(name), user, path, placement, main);
    }
}