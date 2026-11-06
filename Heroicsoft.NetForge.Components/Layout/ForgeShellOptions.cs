using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Static shell content, shared by the nf and Bootstrap component sets. Dynamic sidebars
/// implement <see cref="IShellNavigation"/> instead; when that service is registered its sections
/// replace <see cref="Sections"/>. <see cref="UseSharedMenu"/> draws the shared menu beside those sections.
/// </summary>
public sealed class ForgeShellOptions
{
    /// <summary>
    /// Gets or sets the brand.
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the brand href.
    /// </summary>
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// Gets or sets the logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Gets or sets the logo dark URL.
    /// </summary>
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Gets or sets the mark URL.
    /// </summary>
    public string? MarkUrl { get; set; }

    /// <summary>
    /// Invariant format whose <c>{0}</c> is the escaped search term, for example <c>/?q={0}</c>.
    /// Search stays hidden when this is empty and <see cref="IShellNavigation"/> does not supply one.
    /// </summary>
    public string? SearchUrlFormat { get; set; }

    /// <summary>
    /// Gets or sets the search placeholder.
    /// </summary>
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show theme picker.
    /// </summary>
    public bool ShowThemePicker { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show user menu.
    /// </summary>
    public bool ShowUserMenu { get; set; } = true;

    /// <summary>Extra entries at the top of the user menu, for example Profile. Roles are honoured.</summary>
    public List<ForgeNavItem> UserMenuItems { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show version.
    /// </summary>
    public bool ShowVersion { get; set; } = true;

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Where <see cref="Sections"/> are rendered. <see cref="ForgeNavigationPlacement.Sidebar"/> is
    /// the default. <see cref="ForgeNavigationPlacement.Topbar"/> keeps the links in the top bar
    /// and does not render a sidebar.
    /// </summary>
    public ForgeNavigationPlacement NavigationPlacement { get; set; } = ForgeNavigationPlacement.Sidebar;

    /// <summary>
    /// Gets or sets the sections.
    /// </summary>
    public List<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>
    /// Draws the shared menu beside <see cref="Sections"/>, or beside <see cref="IShellNavigation"/>
    /// when that service is registered (its sections replace <see cref="Sections"/>). A menu that
    /// was never built leaves the shell's own sections in place.
    /// </summary>
    public bool UseSharedMenu { get; set; }

    /// <summary>
    /// Which <c>BuildMenu</c> to draw. Empty is the menu built without a name. The shell shows
    /// that menu on every page that uses this layout; the value does not filter by URL.
    /// </summary>
    public string? SharedMenuName { get; set; }

    /// <summary>
    /// Draws the shared menu after <see cref="Sections"/> when both share a bar. The default draws
    /// the shared menu first. <c>Order</c> on a menu group only sorts links inside the shared menu.
    /// </summary>
    public bool SharedMenuAfterSections { get; set; }

    /// <summary>Component rendered at the end of the top bar, before the user menu. A notification bell, for example.</summary>
    public Type? TopbarEndComponent { get; set; }

    /// <summary>Component pinned at the bottom of the generated sidebar.</summary>
    public Type? SidebarFooterComponent { get; set; }
}

/// <summary>Where the shell draws <see cref="ForgeShellOptions.Sections"/>.</summary>
public enum ForgeNavigationPlacement
{
    /// <summary>
    /// The sidebar value.
    /// </summary>
    Sidebar = 0,
    /// <summary>
    /// The topbar value.
    /// </summary>
    Topbar = 1,
}