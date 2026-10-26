using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

public partial class ForgeNavLink : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Parameter, EditorRequired]
    public ForgeNavItem Item { get; set; } = default!;

    private bool UsesQuery => Item.ActiveQuery is not null || Item.ActivePaths is not null;

    private string Tip => string.IsNullOrWhiteSpace(Item.Tooltip) ? Item.Title : Item.Tooltip;

    private bool IsCurrent =>
        ForgeNavMatch.IsCurrent(Item, Navigation.ToBaseRelativePath(Navigation.Uri));

    private string IconClass =>
        Item.Icon.StartsWith("bi-", StringComparison.Ordinal) || Item.Icon.StartsWith("bi ", StringComparison.Ordinal)
            ? Item.Icon
            : $"bi-{Item.Icon}";

    protected override void OnInitialized()
    {
        if (UsesQuery)
        {
            Navigation.LocationChanged += OnLocationChanged;
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) => InvokeAsync(StateHasChanged);

    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}