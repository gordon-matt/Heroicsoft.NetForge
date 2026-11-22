using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders one nav section in the top bar: a flat list of links when the section has no heading, otherwise a dropdown.
/// </summary>
public partial class ForgeNavMenu : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// The section to draw. Required. Items with roles are shown only to users in those roles. A heading makes a dropdown. A child with no address and with children becomes a nested menu.
    /// </summary>
    [Parameter, EditorRequired]
    public ForgeNavSection Section { get; set; } = default!;

    /// <summary>Shared with the other top-level menus when this section is in the shell top bar.</summary>
    [CascadingParameter]
    private NetForgeMenuBar? MenuBar { get; set; }

    private bool open;
    private string? openChild;
    private bool subscribed;

    private bool UsesBar => MenuBar is not null && !string.IsNullOrWhiteSpace(Section.Heading);

    private bool Open => UsesBar ? MenuBar!.IsOpen(this) : open;

    private string RelativeUrl => Navigation.ToBaseRelativePath(Navigation.Uri);

    private bool AnyCurrent => Section.Items.Any(item =>
        ForgeNavMatch.IsCurrent(item, RelativeUrl)
        || item.Children.Any(child => ForgeNavMatch.IsCurrent(child, RelativeUrl)));

    /// <summary>Closes the dropdown when the address changes.</summary>
    protected override void OnInitialized() => Navigation.LocationChanged += OnLocationChanged;

    /// <summary>Joins the shared menu bar when this section is a top-bar dropdown, so hovering and clicks stay in step with the other menus.</summary>
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
            return;
        }

        open = !open;
    }

    private void OnPointerEnter(PointerEventArgs args)
    {
        if (!UsesBar || !string.Equals(args.PointerType, "mouse", StringComparison.Ordinal))
        {
            return;
        }

        MenuBar!.Hover(this);
    }

    private void ToggleChild(string title) => openChild = openChild == title ? null : title;

    private void Close()
    {
        openChild = null;
        if (UsesBar)
        {
            if (MenuBar!.IsOpen(this))
            {
                MenuBar.Close();
            }

            return;
        }

        open = false;
    }

    private void OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            if (UsesBar)
            {
                MenuBar!.Close();
            }
            else
            {
                open = false;
            }
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
            else
            {
                open = false;
            }

            openChild = null;
            StateHasChanged();
        });

    /// <summary>Stops closing the dropdown on navigation, and leaves the shared menu bar when this section joined it.</summary>
    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        if (subscribed && MenuBar is not null)
        {
            MenuBar.Changed -= OnBarChanged;
        }
    }
}