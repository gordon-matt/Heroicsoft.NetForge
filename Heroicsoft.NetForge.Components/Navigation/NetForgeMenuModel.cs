using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>One named menu after the host's <c>BuildMenu</c> callbacks have run.</summary>
public sealed class NetForgeMenuModel
{
    /// <summary>Creates a menu that renderers can filter by slot and by the current path.</summary>
    /// <param name="name">Menu name. Empty is the unnamed menu.</param>
    /// <param name="isDefined"><see langword="false"/> when nothing was registered for <paramref name="name"/>, so renderers draw nothing.</param>
    /// <param name="placement">Slot used by groups that did not call <c>AsTopbar</c> or <c>AsSidebar</c>.</param>
    /// <param name="sections">The groups, in display order.</param>
    public NetForgeMenuModel(
        string name,
        bool isDefined,
        NetForgeMenuPlacement placement,
        IReadOnlyList<ForgeNavSection> sections)
    {
        Name = name;
        IsDefined = isDefined;
        Placement = placement;
        Sections = sections;
    }

    /// <summary>The name passed to <c>BuildMenu</c>. Empty is the unnamed menu.</summary>
    public string Name { get; }

    /// <summary>False when nothing was registered for <see cref="Name"/>. Renderers draw nothing.</summary>
    public bool IsDefined { get; }

    /// <summary>Slot used by groups that did not choose a side. A group can still pin itself with <c>AsTopbar</c> or <c>AsSidebar</c>.</summary>
    public NetForgeMenuPlacement Placement { get; }

    /// <summary>The groups in this menu, including ones the current user cannot see. Renderers filter them.</summary>
    public IReadOnlyList<ForgeNavSection> Sections { get; }

    /// <summary>
    /// Returns the slot for <paramref name="section"/>. <c>InTopbar</c> <see langword="true"/> is the top bar, <see langword="false"/> is the sidebar, and <see langword="null"/> follows <see cref="Placement"/>.
    /// </summary>
    /// <param name="section">The group to place.</param>
    /// <returns>The slot that group is drawn in.</returns>
    public NetForgeMenuPlacement PlacementOf(ForgeNavSection section) =>
        section.InTopbar switch
        {
            true => NetForgeMenuPlacement.Topbar,
            false => NetForgeMenuPlacement.Sidebar,
            _ => Placement,
        };

    /// <summary>
    /// <see langword="true"/> when any group is in the sidebar. An empty menu is <see langword="true"/> only when <see cref="Placement"/> is the sidebar.
    /// </summary>
    public bool HasSidebarSections =>
        Sections.Count == 0
            ? Placement == NetForgeMenuPlacement.Sidebar
            : Sections.Any(section => PlacementOf(section) == NetForgeMenuPlacement.Sidebar);

    /// <summary>
    /// <see langword="true"/> when any group is in the top bar. An empty menu is <see langword="true"/> only when <see cref="Placement"/> is the top bar.
    /// </summary>
    public bool HasTopbarSections =>
        Sections.Count == 0
            ? Placement == NetForgeMenuPlacement.Topbar
            : Sections.Any(section => PlacementOf(section) == NetForgeMenuPlacement.Topbar);

    /// <summary>
    /// Groups for one slot. A requested placement redraws a menu whose groups did not choose,
    /// and selects one side when a group called <c>AsTopbar</c> or <c>AsSidebar</c>.
    /// </summary>
    /// <param name="requested">The slot being drawn. <see langword="null"/> uses <see cref="Placement"/>.</param>
    /// <param name="sections">Groups to filter. When none of them chose a side, all of them are returned.</param>
    /// <returns>The groups that belong in the requested slot.</returns>
    public IReadOnlyList<ForgeNavSection> SectionsFor(NetForgeMenuPlacement? requested, IReadOnlyList<ForgeNavSection> sections)
    {
        if (sections.All(section => section.InTopbar is null))
        {
            return sections;
        }

        var slot = requested ?? Placement;
        return sections.Where(section => PlacementOf(section) == slot).ToList();
    }

    /// <summary>
    /// The group that contains the current page uses <paramref name="main"/>. Every other group uses the
    /// top bar when <paramref name="main"/> is the sidebar. A group that called <c>AsTopbar</c> or
    /// <c>AsSidebar</c> keeps that choice.
    /// </summary>
    /// <param name="section">The group to place.</param>
    /// <param name="main">Where the group that contains the current page should sit.</param>
    /// <param name="path">The current path, including the query, compared with each link.</param>
    /// <returns>The top bar or the sidebar.</returns>
    public NetForgeMenuPlacement SlotOf(ForgeNavSection section, NetForgeMenuPlacement main, string path) => section.InTopbar == true
            ? NetForgeMenuPlacement.Topbar
            : section.InTopbar == false
            ? NetForgeMenuPlacement.Sidebar
            : main == NetForgeMenuPlacement.Topbar || !IsActive(section, path)
            ? NetForgeMenuPlacement.Topbar
            : NetForgeMenuPlacement.Sidebar;

    /// <summary>
    /// Returns the groups for <paramref name="slot"/> after <see cref="SlotOf"/> runs. A group that does not contain the current page is reduced to one link per item, addressed at that item's first page.
    /// </summary>
    /// <param name="slot">The side being drawn.</param>
    /// <param name="main">Where the group that contains the current page should sit.</param>
    /// <param name="path">The current path, including the query.</param>
    /// <param name="sections">Groups to choose from, usually after the role filter.</param>
    /// <returns>The groups for that slot. Inactive groups are flattened to a single link.</returns>
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

        var home = string.IsNullOrWhiteSpace(item.Href) ? FirstLink(item.Children) : null;

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
        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.Href))
            {
                return item;
            }

            if (item.Children is { Count: > 0 })
            {
                var nested = FirstLink(item.Children);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="path"/> matches a link in <paramref name="section"/> or a nested link under it.
    /// </summary>
    /// <param name="section">The group to test.</param>
    /// <param name="path">The current path, including the query.</param>
    /// <returns><see langword="true"/> when the group contains the current page.</returns>
    public static bool IsActive(ForgeNavSection section, string path) =>
        section.Items.Any(item => ForgeNavMatch.IsInBranch(item, path));

    /// <summary>
    /// Returns a menu that was not built. <see cref="IsDefined"/> is <see langword="false"/>, the placement is the sidebar, and there are no sections, so renderers draw nothing.
    /// </summary>
    /// <param name="name">The name that was requested. <see langword="null"/> or whitespace becomes the unnamed menu.</param>
    /// <returns>The empty menu.</returns>
    public static NetForgeMenuModel Empty(string? name) =>
        new(string.IsNullOrWhiteSpace(name) ? NetForgeMenus.DefaultName : name.Trim(), false, NetForgeMenuPlacement.Sidebar, []);
}