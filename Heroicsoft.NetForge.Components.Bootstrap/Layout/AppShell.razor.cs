using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

public partial class AppShell
{
    private bool sidebarCollapsed;

    [Parameter]
    public string? Brand { get; set; }

    [Parameter]
    public string BrandHref { get; set; } = "/";

    [Parameter]
    public string? LogoUrl { get; set; }

    [Parameter]
    public string? LogoDarkUrl { get; set; }

    [Parameter]
    public string? MarkUrl { get; set; }

    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>The shared menu, drawn with <see cref="Sections"/> when both are set.</summary>
    [Parameter]
    public RenderFragment? Menu { get; set; }

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

    [Parameter]
    public bool ShowSearch { get; set; }

    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>The term currently in the address bar. The box copies this when the page changes.</summary>
    [Parameter]
    public string SearchText { get; set; } = "";

    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    [Parameter]
    public RenderFragment? TopbarEnd { get; set; }

    [Parameter]
    public bool ShowThemePicker { get; set; } = true;

    [Parameter]
    public bool ShowUserMenu { get; set; } = true;

    [Parameter]
    public IReadOnlyList<ForgeNavItem>? UserMenuItems { get; set; }

    [Parameter]
    public bool ShowVersion { get; set; } = true;

    [Parameter]
    public string? Version { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private void ToggleSidebar() => sidebarCollapsed = !sidebarCollapsed;
}