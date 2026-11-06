using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Represents the app shell.
/// </summary>
public partial class AppShell : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    // Two flags because the toggle means different things by width: on a wide screen it
    // collapses the sidebar to an icon rail, on a narrow one it slides the sidebar over the page.
    // CSS applies whichever flag matches the breakpoint.
    private bool sidebarCollapsed;

    private bool sidebarOpen;

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
    /// False makes the brand link a full page load. A library document sets this so the host
    /// address is not handled by the library's own router.
    /// </summary>
    [Parameter]
    public bool EnhanceBrandNavigation { get; set; } = true;

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

    /// <summary>Pinned at the bottom of the generated sidebar, for example a user name and sign-out button.</summary>
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
    /// Replaces the topbar search. Use this when the box is more than a term, for example LogVue's query field.
    /// </summary>
    [Parameter]
    public RenderFragment? TopbarCenter { get; set; }

    /// <summary>
    /// Gets or sets the topbar end.
    /// </summary>
    [Parameter]
    public RenderFragment? TopbarEnd { get; set; }

    /// <summary>
    /// When false, the sidebar stays full width on a wide screen and the toggle only opens it over the page
    /// on a narrow one. Filter sidebars, such as LogVue's, are not usable as an icon rail.
    /// </summary>
    [Parameter]
    public bool CollapsibleSidebar { get; set; } = true;

    /// <summary>Shows the light or dark toggle in the topbar. The name matches the Bootstrap shell.</summary>
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

    /// <inheritdoc/>
    protected override void OnInitialized() => Navigation.LocationChanged += OnLocationChanged;

    private void ToggleSidebar()
    {
        if (CollapsibleSidebar)
        {
            sidebarCollapsed = !sidebarCollapsed;
        }

        sidebarOpen = !sidebarOpen;
    }

    private void CloseSidebar() => sidebarOpen = false;

    // A link tapped in the off-canvas sidebar should reveal the page it opened.
    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        if (!sidebarOpen)
        {
            return;
        }

        sidebarOpen = false;
        _ = InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}