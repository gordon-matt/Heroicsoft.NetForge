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
    [Inject]
    protected INetForgeMenuCatalog Catalog { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IOptions<ForgeUiOptions> UiOptions { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }

    /// <summary>Empty selects the menu built without an area name.</summary>
    [Parameter]
    public string? Area { get; set; }

    /// <summary>
    /// Which slot to draw. Null uses <see cref="ForgeUiOptions.MainMenuPlacement"/>.
    /// The top bar is the groups that are not the current page. The sidebar is that page's group
    /// when the main placement is the sidebar.
    /// </summary>
    [Parameter]
    public NetForgeMenuPlacement? Placement { get; set; }

    [Parameter]
    public string AriaLabel { get; set; } = "Navigation";

    /// <summary>
    /// False when this menu already sits inside another navigation landmark, such as the shell sidebar.
    /// The element stays, without a second "Navigation" label.
    /// </summary>
    [Parameter]
    public bool Landmark { get; set; } = true;

    protected bool IsTop => ResolvedPlacement == NetForgeMenuPlacement.Topbar;

    protected bool Defined { get; private set; }

    protected NetForgeMenuPlacement ResolvedPlacement { get; private set; } = NetForgeMenuPlacement.Sidebar;

    protected IReadOnlyList<ForgeNavSection> Sections { get; private set; } = [];

    /// <summary>Site path plus query, so a menu hosted under another app's base href still matches root links.</summary>
    protected string Path { get; private set; } = "/";

    protected override void OnInitialized() => Navigation.LocationChanged += OnLocationChanged;

    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync();
    }

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
            AuthenticationState state = await AuthenticationState;
            user = state.User;
        }

        NetForgeMenuModel menu = Catalog.Get(Area);
        Defined = menu.IsDefined;
        Path = CurrentPath();
        NetForgeMenuPlacement main = UiOptions.Value.MainMenuPlacement;
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

    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}
