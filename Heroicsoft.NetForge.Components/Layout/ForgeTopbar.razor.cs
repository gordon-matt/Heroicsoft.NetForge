using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders the sidebar toggle or the brand, the top navigation, search, the theme toggle, and the user menu.
/// </summary>
public partial class ForgeTopbar : IDisposable
{
    private readonly NetForgeMenuBar menuBar = new();
    [Inject]
    private IAuthProviderService Auth { get; set; } = default!;

    /// <summary>
    /// Raised with no argument when the sidebar button is clicked. The button is hidden when <see cref="ShowSidebarToggle"/> is <see langword="false"/>.
    /// </summary>
    [Parameter]
    public EventCallback OnToggleSidebar { get; set; }

    /// <summary>
    /// Shows the sidebar button. Defaults to <see langword="true"/>. <see langword="false"/> replaces it with the brand link when a logo or brand name is set.
    /// </summary>
    [Parameter]
    public bool ShowSidebarToggle { get; set; } = true;

    /// <summary>
    /// Host links shown as a top nav. Empty hides that nav. A section with roles is shown only to users in those roles.
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
    /// Name shown in the brand link when the sidebar button is hidden and there is no logo. <see langword="null"/> or whitespace hides that text.
    /// </summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>
    /// Address of that brand link. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// <see langword="false"/> makes the brand link a full page load. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool EnhanceBrandNavigation { get; set; } = true;

    /// <summary>
    /// Brand image used when the sidebar button is hidden. <see langword="null"/> falls through to <see cref="Brand"/>.
    /// </summary>
    [Parameter]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Dark-theme brand image. <see langword="null"/> uses <see cref="LogoUrl"/> for both themes.
    /// </summary>
    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>Sits where the search box would, and takes its place.</summary>
    [Parameter]
    public RenderFragment? CenterContent { get; set; }

    /// <summary>
    /// Shows the search box when <see cref="CenterContent"/> is <see langword="null"/>. Defaults to <see langword="false"/>. The box includes a slash shortcut.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; }

    /// <summary>
    /// Placeholder and accessible name of the search box. Defaults to <c>Search</c>.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>
    /// Text copied into the box when the parent changes it. Defaults to empty. Keystrokes are kept until that value changes.
    /// </summary>
    [Parameter]
    public string SearchText { get; set; } = "";

    /// <summary>
    /// Raised with the trimmed term when the form is submitted and when the box is cleared.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    /// <summary>Shows the light or dark toggle. The name matches the Bootstrap topbar.</summary>
    [Parameter]
    public bool ShowThemePicker { get; set; } = true;

    /// <summary>
    /// Shows the signed-in user's menu, or a Sign in link when nobody is signed in. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ShowUserMenu { get; set; } = true;

    /// <summary>
    /// Links in the user menu. An item with roles is hidden unless the user is in one of them. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavItem>? UserMenuItems { get; set; }

    private List<ForgeNavItem> VisibleUserMenuItems(System.Security.Claims.ClaimsPrincipal user) =>
        (UserMenuItems ?? [])
            .Where(item => string.IsNullOrWhiteSpace(item.Roles)
                || item.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(user.IsInRole))
            .ToList();

    /// <summary>
    /// Markup in the top bar end, before the user menu. <see langword="null"/> renders nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string searchTerm = string.Empty;
    private string appliedSearchText = string.Empty;
    private bool menuOpen;
    private bool navOpen;

    /// <summary>Copies <c>SearchText</c> into the box when the parent changes it, without wiping keystrokes that match the value already applied.</summary>
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

    /// <summary>Subscribes to the menu bar so an open dropdown redraws this bar.</summary>
    protected override void OnInitialized()
    {
        menuBar.Changed += OnMenuBarChanged;
        base.OnInitialized();
    }

    private void OnMenuBarChanged() => _ = InvokeAsync(StateHasChanged);

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

    /// <summary>Stops redrawing when the menu bar changes.</summary>
    public void Dispose() => menuBar.Changed -= OnMenuBarChanged;
}