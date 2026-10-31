using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>Lets the shell render a menu built with <c>BuildMenu</c> without each layout copying the lookup.</summary>
public static class NetForgeMenuShell
{
    public static NetForgeMenuModel? Find(IServiceProvider services, ForgeShellOptions options)
    {
        if (!options.UseSharedMenu)
        {
            return null;
        }

        var catalog = services.GetService<INetForgeMenuCatalog>();
        NetForgeMenuModel? menu = catalog?.Get(options.SharedMenuArea);

        // An area that was never built leaves the shell's own sections in place.
        return menu is { IsDefined: true } ? menu : null;
    }

    /// <summary>True when one area has groups on the side and groups in the top bar.</summary>
    public static bool Splits(NetForgeMenuModel? menu) =>
        menu is { HasSidebarSections: true, HasTopbarSections: true };

    public static ForgeNavigationPlacement Placement(
        NetForgeMenuModel? menu,
        ForgeNavigationPlacement fallback,
        NetForgeMenuPlacement main = NetForgeMenuPlacement.Topbar)
    {
        if (menu is null)
        {
            return fallback;
        }

        // The host asked for its current app down the side, or a group was pinned there.
        if (main == NetForgeMenuPlacement.Sidebar || menu.Sections.Any(section => section.InTopbar == false))
        {
            return ForgeNavigationPlacement.Sidebar;
        }

        return ForgeNavigationPlacement.Topbar;
    }
}
