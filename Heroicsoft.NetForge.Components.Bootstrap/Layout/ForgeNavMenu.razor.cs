using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Represents the forge nav menu.
/// </summary>
public partial class ForgeNavMenu : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// Gets or sets the section.
    /// </summary>
    [Parameter, EditorRequired]
    public ForgeNavSection Section { get; set; } = default!;

    private string RelativeUrl => Navigation.ToBaseRelativePath(Navigation.Uri);

    private bool AnyCurrent => Section.Items.Any(item =>
        ForgeNavMatch.IsCurrent(item, RelativeUrl)
        || item.Children.Any(child => ForgeNavMatch.IsCurrent(child, RelativeUrl)));

    private static string IconClass(ForgeNavItem item) =>
        item.Icon.StartsWith("bi-", StringComparison.Ordinal) || item.Icon.StartsWith("bi ", StringComparison.Ordinal)
            ? item.Icon
            : $"bi-{item.Icon}";

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(Section.Heading))
        {
            Navigation.LocationChanged += OnLocationChanged;
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}