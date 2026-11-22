using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders the Bootstrap application frame: a sidebar or top navigation, a header, the page body,
/// an optional version footer, the theme modal, and the dialog host. Collapsing the sidebar adds
/// <c>bf-sidebar-collapsed</c>.
/// </summary>
public partial class AppShell
{
    private bool sidebarCollapsed;

    /// <summary>
    /// Brand text passed to the sidebar, and to the header when <see cref="NavigationPlacement"/> hides the sidebar.
    /// <see langword="null"/> omits brand text.
    /// </summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>
    /// Address of the brand link in the sidebar and, when the sidebar is hidden, in the header. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// Logo passed to the sidebar and, when the sidebar is hidden, to the header. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Dark-theme logo passed with <see cref="LogoUrl"/>. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Compact mark passed only to the generated sidebar. <see langword="null"/> omits it.
    /// Ignored when <see cref="NavigationPlacement"/> is <see cref="ForgeNavigationPlacement.Topbar"/>.
    /// </summary>
    [Parameter]
    public string? MarkUrl { get; set; }

    /// <summary>
    /// Replaces the logo and brand text inside the generated sidebar. <see langword="null"/> uses the logo or brand.
    /// Ignored when navigation is in the top bar.
    /// </summary>
    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    /// <summary>
    /// Host navigation. With <see cref="ForgeNavigationPlacement.Sidebar"/>, the default, each section is a
    /// sidebar group. With <see cref="ForgeNavigationPlacement.Topbar"/>, the same sections move into the header
    /// and the sidebar is not rendered. An empty list draws no host sections.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>
    /// The shared menu, drawn with <see cref="Sections"/> when both are set.
    /// <see langword="null"/> draws no shared menu. It is placed in the sidebar, or in the header when
    /// <see cref="NavigationPlacement"/> is <see cref="ForgeNavigationPlacement.Topbar"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? Menu { get; set; }

    /// <summary>
    /// Draws <see cref="Menu"/> after <see cref="Sections"/> when both share a bar.
    /// Defaults to <see langword="false"/>, which draws the menu first.
    /// </summary>
    [Parameter]
    public bool SharedMenuAfterSections { get; set; }

    /// <summary>
    /// A second menu drawn in the top bar. <see cref="Menu"/> stays in the sidebar unless
    /// <see cref="NavigationPlacement"/> is <see cref="ForgeNavigationPlacement.Topbar"/>.
    /// <see langword="null"/> draws nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? TopbarMenu { get; set; }

    /// <summary>
    /// Sidebar is the default. <see cref="ForgeNavigationPlacement.Topbar"/> draws <see cref="Sections"/> in the
    /// header instead, hides the sidebar and its toggle, and adds <c>bf-shell-topnav</c>.
    /// </summary>
    [Parameter]
    public ForgeNavigationPlacement NavigationPlacement { get; set; } = ForgeNavigationPlacement.Sidebar;

    /// <summary>
    /// Replaces the generated sidebar. Use this when the nav is data the page owns.
    /// <see langword="null"/> renders the generated sidebar. Ignored when
    /// <see cref="NavigationPlacement"/> is <see cref="ForgeNavigationPlacement.Topbar"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? Sidebar { get; set; }

    /// <summary>
    /// Pinned at the bottom of the generated sidebar. <see langword="null"/> omits it.
    /// Not used when <see cref="Sidebar"/> replaces that sidebar, or when navigation is in the top bar.
    /// </summary>
    [Parameter]
    public RenderFragment? SidebarFooter { get; set; }

    /// <summary>
    /// When <see langword="true"/>, shows the header search box. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; }

    /// <summary>
    /// Placeholder of the header search box. Defaults to <c>Search</c>.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>The term currently in the address bar. The box copies this when the page changes.</summary>
    [Parameter]
    public string SearchText { get; set; } = "";

    /// <summary>
    /// Invoked with the trimmed search text when the header search box is submitted or cleared.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    /// <summary>
    /// Markup in the header, after the theme button and before the user menu. <see langword="null"/> draws nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? TopbarEnd { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the default, shows the theme button and renders one theme modal for the shell.
    /// <see langword="false"/> hides both.
    /// </summary>
    [Parameter]
    public bool ShowThemePicker { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, the default, shows the header user menu.
    /// </summary>
    [Parameter]
    public bool ShowUserMenu { get; set; } = true;

    /// <summary>
    /// Links at the top of the signed-in user menu. <see langword="null"/> draws none.
    /// Items with roles are limited to users in those roles.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavItem>? UserMenuItems { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the default, shows a footer with the version. <see langword="false"/> omits the footer.
    /// </summary>
    [Parameter]
    public bool ShowVersion { get; set; } = true;

    /// <summary>
    /// Version text in the footer, shown with a leading <c>v</c>. <see langword="null"/> uses
    /// <see cref="Heroicsoft.NetForge.Abstractions.AppVersion.Current"/>.
    /// </summary>
    [Parameter]
    public string? Version { get; set; }

    /// <summary>
    /// Page body, rendered in <c>main</c>. <see langword="null"/> leaves that region empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private void ToggleSidebar() => sidebarCollapsed = !sidebarCollapsed;
}