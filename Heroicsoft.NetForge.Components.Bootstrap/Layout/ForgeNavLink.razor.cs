using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders one navigation link as a list item. External addresses open in a new tab with an
/// external-link icon. Items that match a query or extra paths highlight themselves from the current
/// address; other items use <see cref="NavLink"/>, with an exact match when
/// <see cref="ForgeNavItem.MatchAll"/> is set and a prefix match otherwise. In the top bar, hovering
/// the link closes an open dropdown.
/// </summary>
/// <remarks><see cref="ForgeNavItem.Children"/> are not rendered on this link.</remarks>
public partial class ForgeNavLink : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// The link to render. <see cref="ForgeNavItem.Title"/> is the label, and
    /// <see cref="ForgeNavItem.Tooltip"/> is the tooltip when it is not blank.
    /// <see cref="ForgeNavItem.Icon"/> is a Bootstrap Icons class; a value without a <c>bi-</c> or
    /// <c>bi </c> prefix gets <c>bi-</c> prepended. <see cref="ForgeNavItem.TagIcon"/> draws a filled
    /// dot instead. <see cref="ForgeNavItem.External"/> opens a new tab.
    /// <see cref="ForgeNavItem.Count"/> shows a trailing number, or <see cref="ForgeNavItem.Badge"/>
    /// shows a pill using <see cref="ForgeNavItem.BadgeCssClass"/> when the count is not set.
    /// </summary>
    [Parameter, EditorRequired]
    public ForgeNavItem Item { get; set; } = default!;

    /// <summary>Present on a shell top-bar link, so hovering it hides an open dropdown without disarming the bar.</summary>
    [CascadingParameter]
    private NetForgeMenuBar? MenuBar { get; set; }

    private bool UsesQuery => Item.ActiveQuery is not null || Item.ActivePaths is not null;

    private string Tip => string.IsNullOrWhiteSpace(Item.Tooltip) ? Item.Title : Item.Tooltip;

    private bool IsCurrent =>
        ForgeNavMatch.IsCurrent(Item, Navigation.ToBaseRelativePath(Navigation.Uri));

    private string IconClass =>
        Item.Icon.StartsWith("bi-", StringComparison.Ordinal) || Item.Icon.StartsWith("bi ", StringComparison.Ordinal)
            ? Item.Icon
            : $"bi-{Item.Icon}";

    /// <summary>
    /// Subscribes to address changes when the item matches a query or extra paths, so the
    /// <c>active</c> class updates from the current address.
    /// </summary>
    protected override void OnInitialized()
    {
        if (UsesQuery)
        {
            Navigation.LocationChanged += OnLocationChanged;
        }
    }

    private void OnPointerEnter(PointerEventArgs args)
    {
        if (MenuBar is null || !string.Equals(args.PointerType, "mouse", StringComparison.Ordinal))
        {
            return;
        }

        MenuBar.Hover(null);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Stops listening for address changes used to refresh the active state of query-matched links.
    /// </summary>
    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}