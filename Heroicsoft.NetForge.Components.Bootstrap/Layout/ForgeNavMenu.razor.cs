using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

public partial class ForgeNavMenu : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Parameter, EditorRequired]
    public ForgeNavSection Section { get; set; } = default!;

    private string RelativeUrl => Navigation.ToBaseRelativePath(Navigation.Uri);

    private bool AnyCurrent => Section.Items.Any(item => ForgeNavMatch.IsCurrent(item, RelativeUrl));

    private static string IconClass(ForgeNavItem item) =>
        item.Icon.StartsWith("bi-", StringComparison.Ordinal) || item.Icon.StartsWith("bi ", StringComparison.Ordinal)
            ? item.Icon
            : $"bi-{item.Icon}";

    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(Section.Heading))
        {
            Navigation.LocationChanged += OnLocationChanged;
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) => InvokeAsync(StateHasChanged);

    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}