using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>Lets the shell render a menu built with <c>BuildMenu</c> without each layout copying the lookup.</summary>
public static class NetForgeMenuShell
{
    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="options">The options.</param>
    /// <returns>The net forge menu.</returns>
    public static NetForgeMenuModel? Find(IServiceProvider services, ForgeShellOptions options)
    {
        if (!options.UseSharedMenu)
        {
            return null;
        }

        var catalog = services.GetService<INetForgeMenuCatalog>();
        var menu = catalog?.Get(options.SharedMenuName);

        // A menu that was never built leaves the shell's own sections in place.
        return menu is { IsDefined: true } ? menu : null;
    }

    /// <summary>True when one menu has groups on the side and groups in the top bar.</summary>
    public static bool Splits(NetForgeMenuModel? menu) =>
        menu is { HasSidebarSections: true, HasTopbarSections: true };

    /// <summary>
    /// Gets the placement.
    /// </summary>
    /// <param name="menu">The menu.</param>
    /// <param name="fallback">The fallback.</param>
    /// <param name="main">The main.</param>
    /// <returns>The placement.</returns>
    public static ForgeNavigationPlacement Placement(
        NetForgeMenuModel? menu,
        ForgeNavigationPlacement fallback,
        NetForgeMenuPlacement main = NetForgeMenuPlacement.Topbar)
    {
        if (menu is null)
        {
            return fallback;
        }

        // ActiveSharedMenuPlacement is Sidebar, or a group was pinned to the sidebar.
        return main == NetForgeMenuPlacement.Sidebar || menu.Sections.Any(section => section.InTopbar == false)
            ? ForgeNavigationPlacement.Sidebar
            : ForgeNavigationPlacement.Topbar;
    }
}