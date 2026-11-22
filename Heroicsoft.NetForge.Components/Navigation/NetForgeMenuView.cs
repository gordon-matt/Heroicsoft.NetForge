using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Loads one menu for both component packages. The razor file only chooses the markup.</summary>
public abstract class NetForgeMenuView : ComponentBase, IDisposable
{
    private readonly NetForgeMenuBar ownedBar = new();
    private NetForgeMenuBar? subscribed;

    /// <summary>The top bar's bar, when this menu sits in the shell beside other top-level items.</summary>
    [CascadingParameter]
    protected NetForgeMenuBar? SharedBar { get; set; }

    /// <summary>The menus built for this app. <c>Name</c> selects one.</summary>
    [Inject]
    protected INetForgeMenuCatalog Catalog { get; set; } = default!;

    /// <summary>Supplies the current address for <see cref="Path"/> and reloads the menu when it changes.</summary>
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IOptions<ForgeUiOptions> UiOptions { get; set; } = default!;

    /// <summary>
    /// The signed-in user, used to hide links. <see langword="null"/> treats the visitor as anonymous, so role-restricted links are hidden and unrestricted links stay.
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
    /// Accessible name of the navigation element. Defaults to <c>Navigation</c>. Omitted when <see cref="Landmark"/> is <see langword="false"/>.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Navigation";

    /// <summary>
    /// <see langword="false"/> when this menu already sits inside another navigation landmark, such as the shell sidebar.
    /// The element stays, without a second "Navigation" label. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool Landmark { get; set; } = true;

    /// <summary><see langword="true"/> when this menu is drawing the top-bar slot.</summary>
    protected bool IsTop => ResolvedPlacement == NetForgeMenuPlacement.Topbar;

    /// <summary><see langword="false"/> when the named menu was never built, so the component renders nothing.</summary>
    protected bool Defined { get; private set; }

    /// <summary><c>Placement</c> when it is set; otherwise the active shared-menu placement. The sidebar until the first load finishes.</summary>
    protected NetForgeMenuPlacement ResolvedPlacement { get; private set; } = NetForgeMenuPlacement.Sidebar;

    /// <summary>Groups for this slot after role filtering and the active-group split. Empty renders nothing.</summary>
    protected IReadOnlyList<ForgeNavSection> Sections { get; private set; } = [];

    /// <summary>Site path plus query, so a menu hosted under another app's base href still matches root links.</summary>
    protected string Path { get; private set; } = "/";

    /// <summary>One open dropdown for the top-level items. A side menu does not use it.</summary>
    protected NetForgeMenuBar Bar => SharedBar ?? ownedBar;

    /// <summary>False when the shell top bar owns the bar and draws the backdrop.</summary>
    protected bool OwnsBar => SharedBar is null;

    /// <summary>Reloads the menu and closes the open dropdown when the address changes.</summary>
    protected override void OnInitialized() => Navigation.LocationChanged += OnLocationChanged;

    /// <summary>Subscribes to the menu bar that is in effect, so a dropdown opened elsewhere in the bar redraws this menu.</summary>
    protected override void OnParametersSet()
    {
        if (ReferenceEquals(subscribed, Bar))
        {
            return;
        }

        if (subscribed is not null)
        {
            subscribed.Changed -= OnBarChanged;
        }

        subscribed = Bar;
        subscribed.Changed += OnBarChanged;
    }

    /// <summary>Loads the named menu for the current user and address.</summary>
    protected override async Task OnParametersSetAsync() => await LoadAsync();

    /// <summary>
    /// Returns a top-bar item whose children are the section's links and whose title is the section heading. It has no address and no icon.
    /// </summary>
    /// <param name="section">The group to present as one dropdown.</param>
    /// <returns>The synthetic item.</returns>
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

    /// <summary>Closes the open top-level dropdown.</summary>
    protected void CloseBar() => Bar.Close();

    /// <summary>Escape closes the open dropdown and leaves the bar idle.</summary>
    /// <param name="args">The key event.</param>
    protected void OnBarKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            Bar.Close();
        }
    }

    private void OnBarChanged() => _ = InvokeAsync(StateHasChanged);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) =>
        InvokeAsync(async () =>
        {
            Bar.Close();
            await LoadAsync();
            StateHasChanged();
        });

    /// <summary>Stops reloading the menu on navigation and stops listening to the menu bar.</summary>
    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        if (subscribed is not null)
        {
            subscribed.Changed -= OnBarChanged;
        }
    }
}