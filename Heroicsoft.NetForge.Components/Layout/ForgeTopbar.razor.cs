using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Layout;

public partial class ForgeTopbar
{
    [Inject]
    private IAuthProviderService Auth { get; set; } = default!;

    [Parameter]
    public EventCallback OnToggleSidebar { get; set; }

    [Parameter]
    public bool ShowSidebarToggle { get; set; } = true;

    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    [Parameter]
    public string? Brand { get; set; }

    [Parameter]
    public string BrandHref { get; set; } = "/";

    [Parameter]
    public string? LogoUrl { get; set; }

    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>Sits where the search box would, and takes its place.</summary>
    [Parameter]
    public RenderFragment? CenterContent { get; set; }

    [Parameter]
    public bool ShowSearch { get; set; }

    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search";

    [Parameter]
    public string SearchText { get; set; } = "";

    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    /// <summary>Shows the light or dark toggle. The name matches the Bootstrap topbar.</summary>
    [Parameter]
    public bool ShowThemePicker { get; set; } = true;

    [Parameter]
    public bool ShowUserMenu { get; set; } = true;

    [Parameter]
    public IReadOnlyList<ForgeNavItem>? UserMenuItems { get; set; }

    private List<ForgeNavItem> VisibleUserMenuItems(System.Security.Claims.ClaimsPrincipal user) =>
        (UserMenuItems ?? [])
            .Where(item => string.IsNullOrWhiteSpace(item.Roles)
                || item.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(user.IsInRole))
            .ToList();

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string searchTerm = string.Empty;
    private string appliedSearchText = string.Empty;
    private bool menuOpen;
    private bool navOpen;

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