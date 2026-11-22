using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders one <see cref="ForgeNavSection"/> in the top bar. With no heading, the items are
/// <see cref="ForgeNavLink"/> entries. With a heading, they sit in a dropdown that opens on click
/// or mouse hover and closes on Escape, when the address changes, or when another menu in the bar opens.
/// The heading button uses <c>active</c> when any item in the section matches the current address.
/// </summary>
public partial class ForgeNavMenu : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// The links to render. A blank <see cref="ForgeNavSection.Heading"/> renders each item as a
    /// <see cref="ForgeNavLink"/>. Any other heading renders a <c>dropdown</c> titled with that heading.
    /// Items whose <see cref="ForgeNavItem.Roles"/> is set are omitted for users outside those roles.
    /// In a dropdown, a parent with an address is a link and its children are further items in the same menu.
    /// A parent with no address is a <c>dropdown-header</c>. Only that first level of children is listed.
    /// The current item uses <c>active</c> and <c>aria-current="page"</c>.
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

    /// <summary>
    /// Subscribes to address changes when <see cref="Section"/> has a heading, so the dropdown can
    /// close and refresh its active state after navigation.
    /// </summary>
    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(Section.Heading))
        {
            Navigation.LocationChanged += OnLocationChanged;
        }
    }

    /// <summary>
    /// Subscribes once to the shared top-bar menu state when this section is rendered inside that bar.
    /// </summary>
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
    /// Stops listening for address changes and, when this menu joined the shared top-bar state, stops listening for that bar to open or close.
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