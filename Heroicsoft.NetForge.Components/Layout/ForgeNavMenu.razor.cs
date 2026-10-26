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

    private string RelativeUrl => Navigation.ToBaseRelativePath(Navigation.Uri);

    private bool AnyCurrent => Section.Items.Any(item => ForgeNavMatch.IsCurrent(item, RelativeUrl));

    protected override void OnInitialized() => Navigation.LocationChanged += OnLocationChanged;

    private void Toggle() => open = !open;

    private void Close() => open = false;

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
            StateHasChanged();
        });

    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}