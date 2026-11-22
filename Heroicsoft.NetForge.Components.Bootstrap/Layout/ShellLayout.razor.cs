using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Navigation;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders <see cref="AppShell"/> from the registered shell options: brand, navigation, search,
/// theme picker, user menu, version, and the page body. A shared menu is drawn when shell options
/// turn it on and that menu was built. Search is shown only when a search URL format is configured,
/// and submitting it navigates to that URL.
/// </summary>
public partial class ShellLayout : IDisposable
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    [Inject]
    private IOptions<ForgeUiOptions> UiOptions { get; set; } = default!;

    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    [Inject]
    private SidebarRefresh SidebarRefresh { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private readonly CancellationTokenSource lifetime = new();
    private ForgeShellOptions options = new();
    private NetForgeMenuPlacement activeSharedMenuPlacement = NetForgeMenuPlacement.Topbar;
    private IShellNavigation? navigation;
    private NetForgeMenuModel? sharedMenu;
    private IReadOnlyList<ForgeNavSection> sections = [];
    private IReadOnlyList<ShellToolbarItem> toolbarItems = [];
    private string searchText = string.Empty;
    private int loadVersion;

    private bool ShowSearch => !string.IsNullOrWhiteSpace(SearchFormat);

    private string? SearchFormat => navigation?.SearchUrlFormat ?? options.SearchUrlFormat;

    private ForgeNavigationPlacement Placement =>
        NetForgeMenuShell.Placement(sharedMenu, options.NavigationPlacement, activeSharedMenuPlacement);

    private RenderFragment? SharedMenu => sharedMenu is null ? null : builder =>
    {
        var slot = Placement == ForgeNavigationPlacement.Topbar
            ? NetForgeMenuPlacement.Topbar
            : NetForgeMenuPlacement.Sidebar;
        builder.OpenComponent<Heroicsoft.NetForge.Components.Bootstrap.Navigation.NetForgeMenu>(0);
        builder.AddAttribute(1, nameof(Heroicsoft.NetForge.Components.Navigation.NetForgeMenu.Name), sharedMenu.Name);
        builder.AddAttribute(2, nameof(Heroicsoft.NetForge.Components.Navigation.NetForgeMenu.Placement), slot);
        builder.AddAttribute(3, nameof(Heroicsoft.NetForge.Components.Navigation.NetForgeMenu.Landmark), slot != NetForgeMenuPlacement.Sidebar);
        builder.CloseComponent();
    };

    private RenderFragment? TopMenu =>
        sharedMenu is null || Placement == ForgeNavigationPlacement.Topbar
            ? null
            : builder =>
            {
                builder.OpenComponent<Heroicsoft.NetForge.Components.Bootstrap.Navigation.NetForgeMenu>(0);
                builder.AddAttribute(1, nameof(Heroicsoft.NetForge.Components.Navigation.NetForgeMenu.Name), sharedMenu.Name);
                builder.AddAttribute(2, nameof(Heroicsoft.NetForge.Components.Navigation.NetForgeMenu.Placement), NetForgeMenuPlacement.Topbar);
                builder.AddAttribute(3, nameof(Heroicsoft.NetForge.Components.Navigation.NetForgeMenu.Landmark), true);
                builder.CloseComponent();
            };

    private RenderFragment? EndContent => ShellToolbarItems.Render(toolbarItems, options.TopbarEndComponent);

    private RenderFragment? FooterContent => Slot(options.SidebarFooterComponent);

    private static RenderFragment? Slot(Type? type) => type is null ? null : builder =>
    {
        builder.OpenComponent<DynamicComponent>(0);
        builder.AddComponentParameter(1, nameof(DynamicComponent.Type), type);
        builder.CloseComponent();
    };

    /// <summary>
    /// Reads shell options, resolves the shared menu and navigation service, and starts listening
    /// for sidebar refreshes and address changes. The search box is filled from the current address.
    /// </summary>
    protected override void OnInitialized()
    {
        options = ShellOptions.Value;
        activeSharedMenuPlacement = UiOptions.Value.ActiveSharedMenuPlacement;
        navigation = Services.GetService<IShellNavigation>();
        sharedMenu = NetForgeMenuShell.Find(Services, options);
        toolbarItems = ShellToolbarItems.InOrder(Services.GetServices<ShellToolbarItem>());
        sections = navigation is null ? options.Sections : [];
        searchText = CurrentTerm();
        SidebarRefresh.Changed += OnSidebarChanged;
        Navigation.LocationChanged += OnLocationChanged;
    }

    /// <summary>
    /// Loads sidebar sections from the navigation service when one is registered. Static sections
    /// from shell options are left as they are.
    /// </summary>
    /// <returns>
    /// A task that completes when those sections have been loaded, or immediately when no navigation service is registered.
    /// </returns>
    protected override Task OnInitializedAsync() => LoadSectionsAsync();

    private void OnSidebarChanged() => _ = InvokeAsync(LoadSectionsAsync);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) =>
        InvokeAsync(() =>
        {
            searchText = CurrentTerm();
            StateHasChanged();
        });

    private async Task LoadSectionsAsync()
    {
        if (navigation is null)
        {
            return;
        }

        int version = ++loadVersion;

        try
        {
            var next = await navigation.GetSectionsAsync(lifetime.Token);

            if (version == loadVersion)
            {
                sections = next;
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
        {
            return;
        }
    }

    private Task SearchAsync(string term)
    {
        string? trimmed = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        var current = Navigation.ToAbsoluteUri(Navigation.Uri);
        string? url = navigation?.FormatSearchUrl(trimmed, current) ?? ForgeSearchUrl.Build(SearchFormat, trimmed);

        if (!string.IsNullOrWhiteSpace(url))
        {
            Navigation.NavigateTo(url);
        }

        return Task.CompletedTask;
    }

    private string CurrentTerm() =>
        ForgeSearchUrl.ReadTerm(SearchFormat, Navigation.ToAbsoluteUri(Navigation.Uri));

    /// <summary>
    /// Stops listening for sidebar refreshes and address changes, and cancels an in-flight section load.
    /// </summary>
    public void Dispose()
    {
        SidebarRefresh.Changed -= OnSidebarChanged;
        Navigation.LocationChanged -= OnLocationChanged;
        lifetime.Cancel();
        lifetime.Dispose();
    }
}