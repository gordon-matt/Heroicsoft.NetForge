using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Heroicsoft.NetForge.Components.Layout;

public partial class ForgeNavLink : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Parameter, EditorRequired]
    public ForgeNavItem Item { get; set; } = default!;

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