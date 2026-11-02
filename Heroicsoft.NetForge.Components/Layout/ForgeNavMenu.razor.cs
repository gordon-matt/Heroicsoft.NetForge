using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Layout;

public partial class ForgeNavMenu : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Parameter, EditorRequired]
    public ForgeNavSection Section { get; set; } = default!;

    private bool open;
    private string? openChild;

    private string RelativeUrl => Navigation.ToBaseRelativePath(Navigation.Uri);

    private bool AnyCurrent => Section.Items.Any(item =>
        ForgeNavMatch.IsCurrent(item, RelativeUrl)
        || item.Children.Any(child => ForgeNavMatch.IsCurrent(child, RelativeUrl)));

    protected override void OnInitialized() => Navigation.LocationChanged += OnLocationChanged;

    private void Toggle() => open = !open;

    private void ToggleChild(string title) => openChild = openChild == title ? null : title;

    private void Close()
    {
        open = false;
        openChild = null;
    }

    private void OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            open = false;
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) =>
        InvokeAsync(() =>
        {
            open = false;
            openChild = null;
            StateHasChanged();
        });

    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}