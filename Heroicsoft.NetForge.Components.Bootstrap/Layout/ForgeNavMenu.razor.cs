using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

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

    /// <summary>Shared with the other top-level menus when this section is in the shell top bar.</summary>
    [CascadingParameter]
    private NetForgeMenuBar? MenuBar { get; set; }

    private bool subscribed;

    private bool UsesBar => MenuBar is not null && !string.IsNullOrWhiteSpace(Section.Heading);

    private bool Open => UsesBar && MenuBar!.IsOpen(this);

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

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (!UsesBar || subscribed)
        {
            return;
        }

        MenuBar!.Changed += OnBarChanged;
        subscribed = true;
    }

    private void Toggle()
    {
        if (UsesBar)
        {
            MenuBar!.Toggle(this);
        }
    }

    private void OnPointerEnter(PointerEventArgs args)
    {
        if (!UsesBar || !string.Equals(args.PointerType, "mouse", StringComparison.Ordinal))
        {
            return;
        }

        MenuBar!.Hover(this);
    }

    private void OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape" && UsesBar)
        {
            MenuBar!.Close();
        }
    }

    private void OnBarChanged() => _ = InvokeAsync(StateHasChanged);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) =>
        InvokeAsync(() =>
        {
            if (UsesBar)
            {
                MenuBar!.Close();
            }

            StateHasChanged();
        });

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        if (subscribed && MenuBar is not null)
        {
            MenuBar.Changed -= OnBarChanged;
        }
    }
}