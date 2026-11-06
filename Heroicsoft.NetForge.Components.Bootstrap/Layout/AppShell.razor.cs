using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Represents the app shell.
/// </summary>
public partial class AppShell
{
    private bool sidebarCollapsed;

    /// <summary>
    /// Gets or sets the brand.
    /// </summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the brand href.
    /// </summary>
    [Parameter]
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// Gets or sets the logo URL.
    /// </summary>
    [Parameter]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Gets or sets the logo dark URL.
    /// </summary>
    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Gets or sets the mark URL.
    /// </summary>
    [Parameter]
    public string? MarkUrl { get; set; }

    /// <summary>
    /// Gets or sets the brand content.
    /// </summary>
    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    /// <summary>
    /// Gets or sets the sections.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>The shared menu, drawn with <see cref="Sections"/> when both are set.</summary>
    [Parameter]
    public RenderFragment? Menu { get; set; }

    /// <summary>Draws <see cref="Menu"/> after <see cref="Sections"/> when both share a bar.</summary>
    [Parameter]
    public bool SharedMenuAfterSections { get; set; }

    /// <summary>
    /// A second menu drawn in the top bar. <see cref="Menu"/> stays in the sidebar unless
    /// <see cref="NavigationPlacement"/> is <see cref="ForgeNavigationPlacement.Topbar"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? TopbarMenu { get; set; }

    /// <summary>Sidebar is the default. Topbar draws <see cref="Sections"/> in the header instead.</summary>
    [Parameter]
    public ForgeNavigationPlacement NavigationPlacement { get; set; } = ForgeNavigationPlacement.Sidebar;

    /// <summary>Replaces the generated sidebar. Use this when the nav is data the page owns.</summary>
    [Parameter]
    public RenderFragment? Sidebar { get; set; }

    /// <summary>Pinned at the bottom of the generated sidebar.</summary>
    [Parameter]
    public RenderFragment? SidebarFooter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show search.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; }

    /// <summary>
    /// Gets or sets the search placeholder.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>The term currently in the address bar. The box copies this when the page changes.</summary>
    [Parameter]
    public string SearchText { get; set; } = "";

    /// <summary>
    /// Gets or sets the callback raised for the search event.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    /// <summary>
    /// Gets or sets the topbar end.
    /// </summary>
    [Parameter]
    public RenderFragment? TopbarEnd { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show theme picker.
    /// </summary>
    [Parameter]
    public bool ShowThemePicker { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show user menu.
    /// </summary>
    [Parameter]
    public bool ShowUserMenu { get; set; } = true;

    /// <summary>
    /// Gets or sets the user menu items.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavItem>? UserMenuItems { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show version.
    /// </summary>
    [Parameter]
    public bool ShowVersion { get; set; } = true;

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    [Parameter]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private void ToggleSidebar() => sidebarCollapsed = !sidebarCollapsed;
}