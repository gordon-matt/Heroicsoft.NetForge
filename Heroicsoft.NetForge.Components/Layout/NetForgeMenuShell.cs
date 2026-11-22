using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>Lets the shell render a menu built with <c>BuildMenu</c> without each layout copying the lookup.</summary>
public static class NetForgeMenuShell
{
    /// <summary>
    /// Returns the shared menu named by <see cref="ForgeShellOptions.SharedMenuName"/> when <see cref="ForgeShellOptions.UseSharedMenu"/> is set and that menu was built. Otherwise returns <see langword="null"/>, and the shell keeps its own sections.
    /// </summary>
    /// <param name="services">The provider that holds <see cref="INetForgeMenuCatalog"/>. A missing catalog is treated as no menu.</param>
    /// <param name="options">Shell options that name the menu.</param>
    /// <returns>The built menu, or <see langword="null"/>.</returns>
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
    /// Chooses sidebar or top bar for a shared menu. A <see langword="null"/> menu keeps <paramref name="fallback"/>. A menu whose <paramref name="main"/> placement is the sidebar, or that has a group pinned to the sidebar, uses the sidebar. Otherwise the top bar.
    /// </summary>
    /// <param name="menu">The shared menu. <see langword="null"/> returns <paramref name="fallback"/>.</param>
    /// <param name="fallback">Placement used when there is no shared menu.</param>
    /// <param name="main">Where the active group should sit. Defaults to the top bar.</param>
    /// <returns>The placement the shell should use.</returns>
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