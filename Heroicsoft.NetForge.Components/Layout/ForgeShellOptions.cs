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
    /// Name used for the shell brand. <see langword="null"/> or whitespace leaves the brand as the logo, or blank when there is no logo.
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Where the brand link goes. Defaults to <c>/</c>. <see cref="ForgeBrand.Home"/> roots it so a library document cannot keep the click inside the library.
    /// </summary>
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// Brand image for the light theme. <see langword="null"/> shows <see cref="Brand"/> instead.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Brand image for the dark theme. <see langword="null"/> uses <see cref="LogoUrl"/>.
    /// </summary>
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Compact image for the collapsed sidebar. <see langword="null"/> hides it.
    /// </summary>
    public string? MarkUrl { get; set; }

    /// <summary>
    /// Invariant format whose <c>{0}</c> is the escaped search term, for example <c>/?q={0}</c>.
    /// Search stays hidden when this is empty and <see cref="IShellNavigation"/> does not supply one.
    /// </summary>
    public string? SearchUrlFormat { get; set; }

    /// <summary>
    /// Placeholder of the shell search box. Defaults to <c>Search</c>.
    /// </summary>
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>
    /// Shows the light or dark toggle. Defaults to <see langword="true"/>. <see langword="false"/> hides it.
    /// </summary>
    public bool ShowThemePicker { get; set; } = true;

    /// <summary>
    /// Shows the user menu, or a Sign in link when nobody is signed in. Defaults to <see langword="true"/>. <see langword="false"/> hides both.
    /// </summary>
    public bool ShowUserMenu { get; set; } = true;

    /// <summary>Extra entries at the top of the user menu, for example Profile. Roles are honoured.</summary>
    public List<ForgeNavItem> UserMenuItems { get; set; } = [];

    /// <summary>
    /// Shows the version footer. Defaults to <see langword="true"/>. <see langword="false"/> hides it.
    /// </summary>
    public bool ShowVersion { get; set; } = true;

    /// <summary>
    /// Text in the version footer. <see langword="null"/> uses the running assembly's informational version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Where <see cref="Sections"/> are rendered. <see cref="ForgeNavigationPlacement.Sidebar"/> is
    /// the default. <see cref="ForgeNavigationPlacement.Topbar"/> keeps the links in the top bar
    /// and does not render a sidebar.
    /// </summary>
    public ForgeNavigationPlacement NavigationPlacement { get; set; } = ForgeNavigationPlacement.Sidebar;

    /// <summary>
    /// Host links drawn by the shell. An empty list shows none. A registered <see cref="IShellNavigation"/> replaces this list.
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
    /// <summary>The links run down the sidebar. This is the default, and the shell still renders a sidebar.</summary>
    Sidebar = 0,

    /// <summary>The links run across the top bar, and the shell does not render a sidebar.</summary>
    Topbar = 1,
}