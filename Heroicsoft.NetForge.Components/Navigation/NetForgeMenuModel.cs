using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>One named menu after the host's <c>BuildMenu</c> callbacks have run.</summary>
public sealed class NetForgeMenuModel
{
    public NetForgeMenuModel(
        string area,
        bool isDefined,
        NetForgeMenuPlacement placement,
        IReadOnlyList<ForgeNavSection> sections)
    {
        Area = area;
        IsDefined = isDefined;
        Placement = placement;
        Sections = sections;
    }

    public string Area { get; }

    /// <summary>False when nothing was registered for <see cref="Area"/>. Renderers draw nothing.</summary>
    public bool IsDefined { get; }

    public NetForgeMenuPlacement Placement { get; }

    public IReadOnlyList<ForgeNavSection> Sections { get; }

    public NetForgeMenuPlacement PlacementOf(ForgeNavSection section) =>
        section.InTopbar switch
        {
            true => NetForgeMenuPlacement.Topbar,
            false => NetForgeMenuPlacement.Sidebar,
            _ => Placement,
        };

    public bool HasSidebarSections =>
        Sections.Count == 0
            ? Placement == NetForgeMenuPlacement.Sidebar
            : Sections.Any(section => PlacementOf(section) == NetForgeMenuPlacement.Sidebar);

    public bool HasTopbarSections =>
        Sections.Count == 0
            ? Placement == NetForgeMenuPlacement.Topbar
            : Sections.Any(section => PlacementOf(section) == NetForgeMenuPlacement.Topbar);

    /// <summary>
    /// Groups for one slot. A requested placement redraws a menu whose groups did not choose,
    /// and selects one side when a group called <c>AsTopbar</c> or <c>AsSidebar</c>.
    /// </summary>
    public IReadOnlyList<ForgeNavSection> SectionsFor(NetForgeMenuPlacement? requested, IReadOnlyList<ForgeNavSection> sections)
    {
        if (sections.All(section => section.InTopbar is null))
        {
            return sections;
        }

        NetForgeMenuPlacement slot = requested ?? Placement;
        return sections.Where(section => PlacementOf(section) == slot).ToList();
    }

    /// <summary>
    /// The group that contains the current page uses <paramref name="main"/>. Every other group uses the
    /// top bar when <paramref name="main"/> is the sidebar. A group that called <c>AsTopbar</c> or
    /// <c>AsSidebar</c> keeps that choice.
    /// </summary>
    public NetForgeMenuPlacement SlotOf(ForgeNavSection section, NetForgeMenuPlacement main, string path)
    {
        if (section.InTopbar == true)
        {
            return NetForgeMenuPlacement.Topbar;
        }

        if (section.InTopbar == false)
        {
            return NetForgeMenuPlacement.Sidebar;
        }

        if (main == NetForgeMenuPlacement.Topbar || !IsActive(section, path))
        {
            return NetForgeMenuPlacement.Topbar;
        }

        return NetForgeMenuPlacement.Sidebar;
    }

    public IReadOnlyList<ForgeNavSection> ForSlot(
        NetForgeMenuPlacement slot,
        NetForgeMenuPlacement main,
        string path,
        IReadOnlyList<ForgeNavSection> sections) =>
        sections
            .Where(section => SlotOf(section, main, path) == slot)
            .Select(section => IsActive(section, path) ? section : AsOutsideLink(section))
            .ToList();

    /// <summary>
    /// A submenu seen from another app is one link, titled with the app name and addressed at its first page.
    /// The full tree stays on the section that contains the current page.
    /// </summary>
    private static ForgeNavSection AsOutsideLink(ForgeNavSection section) =>
        new()
        {
            Heading = section.Heading,
            Roles = section.Roles,
            InTopbar = section.InTopbar,
            Items = section.Items.Select(AsSingleLink).ToList(),
        };

    private static ForgeNavItem AsSingleLink(ForgeNavItem item)
    {
        if (item.Children is not { Count: > 0 })
        {
            return item;
        }

        ForgeNavItem? home = string.IsNullOrWhiteSpace(item.Href) ? FirstLink(item.Children) : null;

        return new ForgeNavItem
        {
            Title = item.Title,
            Href = home?.Href ?? item.Href,
            Icon = item.Icon,
            Tooltip = item.Tooltip,
            External = item.External,
            Roles = item.Roles,
            CssClass = item.CssClass,
            MatchAll = home?.MatchAll ?? item.MatchAll,
            Children = [],
        };
    }

    private static ForgeNavItem? FirstLink(IReadOnlyList<ForgeNavItem> items)
    {
        foreach (ForgeNavItem item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.Href))
            {
                return item;
            }

            if (item.Children is { Count: > 0 })
            {
                ForgeNavItem? nested = FirstLink(item.Children);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    public static bool IsActive(ForgeNavSection section, string path) =>
        section.Items.Any(item => ForgeNavMatch.IsInBranch(item, path));

    public static NetForgeMenuModel Empty(string? area) =>
        new(string.IsNullOrWhiteSpace(area) ? NetForgeMenus.DefaultArea : area.Trim(), false, NetForgeMenuPlacement.Sidebar, []);
}
