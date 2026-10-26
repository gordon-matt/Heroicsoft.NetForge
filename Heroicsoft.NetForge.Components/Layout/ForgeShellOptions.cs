using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Static shell content, shared by the nf and Bootstrap component sets. Dynamic sidebars
/// implement <see cref="IShellNavigation"/> instead; when that service is registered its sections
/// replace <see cref="Sections"/>.
/// </summary>
public sealed class ForgeShellOptions
{
    public string? Brand { get; set; }

    public string BrandHref { get; set; } = "/";

    public string? LogoUrl { get; set; }

    public string? LogoDarkUrl { get; set; }

    public string? MarkUrl { get; set; }

    /// <summary>
    /// Invariant format whose <c>{0}</c> is the escaped search term, for example <c>/?q={0}</c>.
    /// Search stays hidden when this is empty and <see cref="IShellNavigation"/> does not supply one.
    /// </summary>
    public string? SearchUrlFormat { get; set; }

    public string SearchPlaceholder { get; set; } = "Search";

    public bool ShowThemePicker { get; set; } = true;

    public bool ShowUserMenu { get; set; } = true;

    /// <summary>Extra entries at the top of the user menu, for example Profile. Roles are honoured.</summary>
    public List<ForgeNavItem> UserMenuItems { get; set; } = [];

    public bool ShowVersion { get; set; } = true;

    public string? Version { get; set; }

    /// <summary>
    /// Where <see cref="Sections"/> are rendered. <see cref="ForgeNavigationPlacement.Sidebar"/> is
    /// the default. <see cref="ForgeNavigationPlacement.Topbar"/> keeps the links in the top bar
    /// and does not render a sidebar.
    /// </summary>
    public ForgeNavigationPlacement NavigationPlacement { get; set; } = ForgeNavigationPlacement.Sidebar;

    public List<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>Component rendered at the end of the top bar, before the user menu. A notification bell, for example.</summary>
    public Type? TopbarEndComponent { get; set; }

    /// <summary>Component pinned at the bottom of the generated sidebar.</summary>
    public Type? SidebarFooterComponent { get; set; }
}

/// <summary>Where the shell draws <see cref="ForgeShellOptions.Sections"/>.</summary>
public enum ForgeNavigationPlacement
{
    Sidebar = 0,
    Topbar = 1,
}