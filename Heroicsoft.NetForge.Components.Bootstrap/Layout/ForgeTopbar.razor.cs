using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders the shell header: an optional sidebar toggle or brand, top navigation, an optional
/// search box, the theme button, caller content, and the user menu.
/// </summary>
public partial class ForgeTopbar : IDisposable
{
    private readonly NetForgeMenuBar menuBar = new();
    [Inject]
    private IAuthProviderService Auth { get; set; } = default!;

    /// <summary>
    /// Invoked when the sidebar toggle button is clicked. That button is shown when
    /// <see cref="ShowSidebarToggle"/> is <see langword="true"/>.
    /// </summary>
    [Parameter]
    public EventCallback OnToggleSidebar { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the default, shows a list-icon button that raises <see cref="OnToggleSidebar"/>.
    /// When <see langword="false"/>, a brand link is shown instead when <see cref="Brand"/> or <see cref="LogoUrl"/>
    /// is set, plus a menu button on small screens that expands <see cref="Sections"/>.
    /// </summary>
    [Parameter]
    public bool ShowSidebarToggle { get; set; } = true;

    /// <summary>
    /// Navigation groups rendered as a <c>nav bf-topnav</c> of <see cref="ForgeNavMenu"/> items.
    /// An empty list, the default, draws no host navigation. A section whose <see cref="ForgeNavSection.Roles"/>
    /// is set is omitted for users outside those roles.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>
    /// Extra navigation rendered with <see cref="Sections"/>. <see langword="null"/>, the default, draws nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? Menu { get; set; }

    /// <summary>
    /// A menu that stays in the top bar while <see cref="Menu"/> is in the sidebar.
    /// <see langword="null"/> draws nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? TopbarMenu { get; set; }

    /// <summary>
    /// Draws <see cref="Menu"/> after <see cref="Sections"/>. Defaults to <see langword="false"/>, which draws the menu first.
    /// </summary>
    [Parameter]
    public bool SharedMenuAfterSections { get; set; }

    /// <summary>
    /// Text shown in the brand link when neither <see cref="LogoUrl"/> nor <see cref="LogoDarkUrl"/> is set,
    /// and the image alt text when a logo is set. <see langword="null"/> draws no brand text.
    /// The brand link appears only when <see cref="ShowSidebarToggle"/> is <see langword="false"/>.
    /// </summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>
    /// Address of the brand link. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// Light-theme logo in the brand link. <see langword="null"/> omits that image.
    /// </summary>
    [Parameter]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Dark-theme logo shown with <see cref="LogoUrl"/>. <see langword="null"/> omits that image.
    /// When both logo URLs are <see langword="null"/>, <see cref="Brand"/> is shown as text.
    /// </summary>
    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// When <see langword="true"/>, shows a search box in the header. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; }

    /// <summary>
    /// Placeholder and accessible name of the search box. Defaults to <c>Search</c>.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>
    /// Text copied into the search box when it changes. Defaults to empty. The box keeps what the
    /// user is typing until this value changes.
    /// </summary>
    [Parameter]
    public string SearchText { get; set; } = "";

    /// <summary>
    /// Invoked with the trimmed search text when the search form is submitted, and also when the box is cleared.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the default, shows the palette button that opens the theme modal.
    /// </summary>
    [Parameter]
    public bool ShowThemePicker { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, the default, shows the user menu. A signed-in user gets a person-icon
    /// dropdown. Its last row posts a sign-out form: Keycloak posts to <c>/auth/logout</c>, and any other
    /// provider posts to the Identity logout page. When the app has no authentication provider, that row
    /// says the app is running without authentication. A signed-out user gets a Sign in button, addressed
    /// at Keycloak, Identity, or <c>/</c> for no provider. <see langword="false"/> hides both.
    /// </summary>
    [Parameter]
    public bool ShowUserMenu { get; set; } = true;

    /// <summary>
    /// Links at the top of the signed-in user dropdown. <see langword="null"/> or empty draws none.
    /// An item whose <see cref="ForgeNavItem.Roles"/> is set is omitted unless the user is in one of those roles.
    /// Each item shows <see cref="ForgeNavItem.Title"/>, <see cref="ForgeNavItem.Href"/>, and a Bootstrap icon
    /// from <see cref="ForgeNavItem.Icon"/>.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavItem>? UserMenuItems { get; set; }

    private List<ForgeNavItem> VisibleUserMenuItems(System.Security.Claims.ClaimsPrincipal user) =>
        (UserMenuItems ?? [])
            .Where(item => string.IsNullOrWhiteSpace(item.Roles)
                || item.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(user.IsInRole))
            .ToList();

    /// <summary>
    /// Markup placed after the theme button and before the user menu. <see langword="null"/> draws nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string searchTerm = string.Empty;
    private string appliedSearchText = string.Empty;

    private bool navOpen;

    /// <summary>
    /// Copies <see cref="SearchText"/> into the search box when the supplied text changes.
    /// </summary>
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

    private async Task OnSearchChanged(string? value)
    {
        searchTerm = value ?? string.Empty;

        // The search box's clear button empties the field without submitting the form.
        if (searchTerm.Length == 0)
        {
            await SubmitSearch();
        }
    }

    /// <summary>
    /// Subscribes to the shared menu bar so an open dropdown can dim the page and close together with the others.
    /// </summary>
    protected override void OnInitialized()
    {
        menuBar.Changed += OnMenuBarChanged;
        base.OnInitialized();
    }

    private void OnMenuBarChanged() => _ = InvokeAsync(StateHasChanged);

    private Task SubmitSearch() => OnSearch.InvokeAsync(searchTerm.Trim());

    /// <summary>
    /// Stops listening for the shared menu bar to open or close.
    /// </summary>
    public void Dispose() => menuBar.Changed -= OnMenuBarChanged;
}