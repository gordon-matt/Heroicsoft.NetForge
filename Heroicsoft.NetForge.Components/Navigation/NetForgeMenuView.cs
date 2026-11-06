using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Loads one menu for both component packages. The razor file only chooses the markup.</summary>
public abstract class NetForgeMenuView : ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the catalog.
    /// </summary>
    [Inject]
    protected INetForgeMenuCatalog Catalog { get; set; } = default!;

    /// <summary>
    /// Gets or sets the navigation.
    /// </summary>
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IOptions<ForgeUiOptions> UiOptions { get; set; } = default!;

    /// <summary>
    /// Gets or sets the authentication state.
    /// </summary>
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }

    /// <summary>Which <c>BuildMenu</c> to draw. Empty selects the menu built without a name.</summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Which slot to draw. Null uses <see cref="ForgeUiOptions.ActiveSharedMenuPlacement"/>.
    /// The top bar is the groups that are not the current page. The sidebar is that page's group
    /// when the main placement is the sidebar.
    /// </summary>
    [Parameter]
    public NetForgeMenuPlacement? Placement { get; set; }

    /// <summary>
    /// Gets or sets the aria label.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Navigation";

    /// <summary>
    /// False when this menu already sits inside another navigation landmark, such as the shell sidebar.
    /// The element stays, without a second "Navigation" label.
    /// </summary>
    [Parameter]
    public bool Landmark { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether this instance is top.
    /// </summary>
    protected bool IsTop => ResolvedPlacement == NetForgeMenuPlacement.Topbar;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is defined.
    /// </summary>
    protected bool Defined { get; private set; }

    /// <summary>
    /// Gets or sets the resolved placement.
    /// </summary>
    protected NetForgeMenuPlacement ResolvedPlacement { get; private set; } = NetForgeMenuPlacement.Sidebar;

    /// <summary>
    /// Gets or sets the sections.
    /// </summary>
    protected IReadOnlyList<ForgeNavSection> Sections { get; private set; } = [];

    /// <summary>Site path plus query, so a menu hosted under another app's base href still matches root links.</summary>
    protected string Path { get; private set; } = "/";

    /// <inheritdoc/>
    protected override void OnInitialized() => Navigation.LocationChanged += OnLocationChanged;

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync() => await LoadAsync();

    /// <summary>
    /// Gets the section item.
    /// </summary>
    /// <param name="section">The section.</param>
    /// <returns>The section item.</returns>
    protected static ForgeNavItem SectionItem(ForgeNavSection section) =>
        new()
        {
            Title = section.Heading ?? string.Empty,
            Href = string.Empty,
            Icon = string.Empty,
            Children = section.Items,
        };

    private async Task LoadAsync()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity());

        if (AuthenticationState is not null)
        {
            var state = await AuthenticationState;
            user = state.User;
        }

        var menu = Catalog.Get(Name);
        Defined = menu.IsDefined;
        Path = CurrentPath();
        var main = UiOptions.Value.ActiveSharedMenuPlacement;
        ResolvedPlacement = Placement ?? main;
        Sections = menu.ForSlot(ResolvedPlacement, main, Path, NetForgeMenuFilter.Apply(menu.Sections, user));
    }

    private string CurrentPath()
    {
        var uri = new Uri(Navigation.Uri);
        return uri.PathAndQuery;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) =>
        InvokeAsync(async () =>
        {
            await LoadAsync();
            StateHasChanged();
        });

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}