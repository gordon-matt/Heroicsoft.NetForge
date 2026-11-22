using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders the sidebar or top navigation, the top bar, the page body, an optional version footer, and the dialog host.
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
    /// Name shown when there is no logo and no <see cref="BrandContent"/>. Also the alt text of the logo. <see langword="null"/> or whitespace hides the text.
    /// </summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>
    /// Address of the brand link. Defaults to <c>/</c>.
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
    /// Image shown as the brand. <see langword="null"/> falls through to the brand text when <see cref="BrandContent"/> is <see langword="null"/>.
    /// </summary>
    [Parameter]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Image used in the dark theme. <see langword="null"/> uses <see cref="LogoUrl"/> for both themes. When set, both images are rendered and the stylesheet swaps them.
    /// </summary>
    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Compact mark shown beside the logo, including when the sidebar is collapsed to an icon rail. <see langword="null"/> hides it.
    /// </summary>
    [Parameter]
    public string? MarkUrl { get; set; }

    /// <summary>
    /// Replaces the logo and brand text. <see langword="null"/> uses the logo or the name.
    /// </summary>
    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    /// <summary>
    /// Links in the generated sidebar, or in the top bar when <see cref="NavigationPlacement"/> is <see cref="ForgeNavigationPlacement.Topbar"/>. A custom <see cref="Sidebar"/> replaces the generated sidebar, so these links are not drawn there. Defaults to an empty list.
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
    /// Shows the search box when <see cref="TopbarCenter"/> is <see langword="null"/>. Defaults to <see langword="false"/>, which hides it.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; }

    /// <summary>
    /// Placeholder and accessible name of the search box. Defaults to <c>Search</c>.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>The term currently in the address bar. The box copies this when the page changes.</summary>
    [Parameter]
    public string SearchText { get; set; } = "";

    /// <summary>
    /// Raised with the trimmed search term when the search form is submitted and when the box is cleared.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    /// <summary>
    /// Replaces the topbar search. Use this when the box is more than a term, for example LogVue's query field.
    /// </summary>
    [Parameter]
    public RenderFragment? TopbarCenter { get; set; }

    /// <summary>
    /// Markup at the end of the top bar, before the user menu. <see langword="null"/> renders nothing.
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
    /// Shows the signed-in user's menu, or a Sign in link when nobody is signed in. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ShowUserMenu { get; set; } = true;

    /// <summary>
    /// Extra links in the user menu. An item with roles is hidden unless the user is in one of them. <see langword="null"/> or empty adds no links beyond Sign out.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavItem>? UserMenuItems { get; set; }

    /// <summary>
    /// Shows the version footer. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ShowVersion { get; set; } = true;

    /// <summary>
    /// Text after <c>v</c> in the footer. <see langword="null"/> uses the running assembly's informational version.
    /// </summary>
    [Parameter]
    public string? Version { get; set; }

    /// <summary>
    /// The page body, inside the main region. <see langword="null"/> leaves that region empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Starts closing the off-canvas sidebar when navigation changes the address.</summary>
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

    /// <summary>Stops closing the off-canvas sidebar on later navigations.</summary>
    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}