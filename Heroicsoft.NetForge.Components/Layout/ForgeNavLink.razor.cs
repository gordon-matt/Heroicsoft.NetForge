using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders one sidebar or top-bar link, including its icon, badge, count, or badge component, and marks it current.
/// </summary>
public partial class ForgeNavLink : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// The link to draw. Required. An external link opens in a new tab. A link with an active query or extra paths tracks the address itself; other links use prefix matching unless the item asks for an exact match.
    /// </summary>
    [Parameter, EditorRequired]
    public ForgeNavItem Item { get; set; } = default!;

    /// <summary>Present on a shell top-bar link, so hovering it hides an open dropdown without disarming the bar.</summary>
    [CascadingParameter]
    private NetForgeMenuBar? MenuBar { get; set; }

    private bool UsesQuery => Item.ActiveQuery is not null || Item.ActivePaths is not null;

    private string Tip => string.IsNullOrWhiteSpace(Item.Tooltip) ? Item.Title : Item.Tooltip;

    private RenderFragment Trailing => builder =>
    {
        if (Item.BadgeComponent is Type type)
        {
            builder.OpenComponent<DynamicComponent>(0);
            builder.AddAttribute(1, "Type", type);
            if (Item.BadgeParameters is not null)
            {
                builder.AddAttribute(2, "Parameters", Item.BadgeParameters);
            }

            builder.CloseComponent();
            return;
        }

        if (Item.Count is int count)
        {
            builder.OpenElement(3, "span");
            builder.AddAttribute(4, "class", "nf-nav-count");
            builder.AddContent(5, count.ToString("N0"));
            builder.CloseElement();
            return;
        }

        if (!string.IsNullOrEmpty(Item.Badge))
        {
            builder.OpenElement(6, "span");
            builder.AddAttribute(7, "class", "nf-badge nf-nav-badge " + ForgeBadge.ToneClass(Item.BadgeCssClass));
            builder.AddContent(8, Item.Badge);
            builder.CloseElement();
        }
    };

    private bool IsCurrent =>
        ForgeNavMatch.IsCurrent(Item, Navigation.ToBaseRelativePath(Navigation.Uri));

    /// <summary>Subscribes to address changes when the item matches a query or extra paths, so the active style updates.</summary>
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

    /// <summary>Stops updating the active style from later address changes.</summary>
    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}