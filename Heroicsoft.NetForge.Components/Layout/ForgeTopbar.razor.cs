using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Represents the forge topbar.
/// </summary>
public partial class ForgeTopbar
{
    [Inject]
    private IAuthProviderService Auth { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback raised for the toggle sidebar event.
    /// </summary>
    [Parameter]
    public EventCallback OnToggleSidebar { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show sidebar toggle.
    /// </summary>
    [Parameter]
    public bool ShowSidebarToggle { get; set; } = true;

    /// <summary>
    /// Gets or sets the sections.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>Replaces <see cref="Sections"/> when the shell is showing a shared menu across the top.</summary>
    [Parameter]
    public RenderFragment? Menu { get; set; }

    /// <summary>A menu that stays in the top bar while <see cref="Menu"/> is in the sidebar.</summary>
    [Parameter]
    public RenderFragment? TopbarMenu { get; set; }

    /// <summary>Draws <see cref="Menu"/> after <see cref="Sections"/>.</summary>
    [Parameter]
    public bool SharedMenuAfterSections { get; set; }

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
    /// Gets or sets a value indicating whether this instance is enhance brand navigation.
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

    /// <summary>Sits where the search box would, and takes its place.</summary>
    [Parameter]
    public RenderFragment? CenterContent { get; set; }

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

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    [Parameter]
    public string SearchText { get; set; } = "";

    /// <summary>
    /// Gets or sets the callback raised for the search event.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    /// <summary>Shows the light or dark toggle. The name matches the Bootstrap topbar.</summary>
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

    private List<ForgeNavItem> VisibleUserMenuItems(System.Security.Claims.ClaimsPrincipal user) =>
        (UserMenuItems ?? [])
            .Where(item => string.IsNullOrWhiteSpace(item.Roles)
                || item.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(user.IsInRole))
            .ToList();

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string searchTerm = string.Empty;
    private string appliedSearchText = string.Empty;
    private bool menuOpen;
    private bool navOpen;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (!string.Equals(SearchText, appliedSearchText, StringComparison.Ordinal))
        {
            appliedSearchText = SearchText ?? string.Empty;
            searchTerm = appliedSearchText;
        }
    }

    private string LogoutUrl => Auth.Provider == AuthProvider.Keycloak
        ? "/auth/logout"
        : "/Identity/Account/Logout";

    private string LoginHref => Auth.Provider switch
    {
        AuthProvider.Keycloak => "/auth/login",
        AuthProvider.Identity => "/Identity/Account/Login",
        _ => "/",
    };

    private void ToggleMenu() => menuOpen = !menuOpen;

    private void CloseMenu() => menuOpen = false;

    private void OnMenuKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            menuOpen = false;
        }
    }

    private async Task OnSearchChanged(string? value)
    {
        searchTerm = value ?? string.Empty;

        // The search box's clear button empties the field without submitting the form.
        if (searchTerm.Length == 0)
        {
            await SubmitSearch();
        }
    }

    private Task SubmitSearch() => OnSearch.InvokeAsync(searchTerm.Trim());
}