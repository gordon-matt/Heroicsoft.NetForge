using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Layout;

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
    private NetForgeMenuPlacement mainPlacement = NetForgeMenuPlacement.Topbar;
    private IShellNavigation? navigation;
    private NetForgeMenuModel? sharedMenu;
    private IReadOnlyList<ForgeNavSection> sections = [];
    private string searchText = string.Empty;
    private int loadVersion;

    private bool ShowSearch => !string.IsNullOrWhiteSpace(SearchFormat);

    private string? SearchFormat => navigation?.SearchUrlFormat ?? options.SearchUrlFormat;

    private ForgeNavigationPlacement Placement =>
        NetForgeMenuShell.Placement(sharedMenu, options.NavigationPlacement, mainPlacement);

    private RenderFragment? SharedMenu => sharedMenu is null ? null : builder =>
    {
        NetForgeMenuPlacement slot = Placement == ForgeNavigationPlacement.Topbar
            ? NetForgeMenuPlacement.Topbar
            : NetForgeMenuPlacement.Sidebar;
        builder.OpenComponent<NetForgeMenu>(0);
        builder.AddAttribute(1, nameof(NetForgeMenu.Area), sharedMenu.Area);
        builder.AddAttribute(2, nameof(NetForgeMenu.Placement), slot);
        builder.AddAttribute(3, nameof(NetForgeMenu.Landmark), slot != NetForgeMenuPlacement.Sidebar);
        builder.CloseComponent();
    };

    private RenderFragment? TopMenu =>
        sharedMenu is null || Placement == ForgeNavigationPlacement.Topbar
            ? null
            : builder =>
            {
                builder.OpenComponent<NetForgeMenu>(0);
                builder.AddAttribute(1, nameof(NetForgeMenu.Area), sharedMenu.Area);
                builder.AddAttribute(2, nameof(NetForgeMenu.Placement), NetForgeMenuPlacement.Topbar);
                builder.AddAttribute(3, nameof(NetForgeMenu.Landmark), true);
                builder.CloseComponent();
            };

    private RenderFragment? EndContent => Slot(options.TopbarEndComponent);

    private RenderFragment? FooterContent => Slot(options.SidebarFooterComponent);

    private static RenderFragment? Slot(Type? type) => type is null ? null : builder =>
    {
        builder.OpenComponent<DynamicComponent>(0);
        builder.AddComponentParameter(1, nameof(DynamicComponent.Type), type);
        builder.CloseComponent();
    };

    protected override void OnInitialized()
    {
        options = ShellOptions.Value;
        mainPlacement = UiOptions.Value.MainMenuPlacement;
        navigation = Services.GetService<IShellNavigation>();
        sharedMenu = NetForgeMenuShell.Find(Services, options);
        sections = navigation is null && sharedMenu is null ? options.Sections : [];
        searchText = CurrentTerm();
        SidebarRefresh.Changed += OnSidebarChanged;
        Navigation.LocationChanged += OnLocationChanged;
    }

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

    public void Dispose()
    {
        SidebarRefresh.Changed -= OnSidebarChanged;
        Navigation.LocationChanged -= OnLocationChanged;
        lifetime.Cancel();
        lifetime.Dispose();
    }
}