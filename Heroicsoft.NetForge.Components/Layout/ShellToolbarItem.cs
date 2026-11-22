using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// One component drawn in the top bar, after the theme toggle and before the user menu.
/// A library registers its own icon here so it does not have to replace <see cref="ForgeShellOptions.TopbarEndComponent"/>.
/// </summary>
/// <param name="ComponentType">The component. It draws its own icon and any menu that icon opens.</param>
/// <param name="Order">Lower values sit closer to the theme toggle. Equal orders keep registration order.</param>
public sealed record ShellToolbarItem(Type ComponentType, int Order);

/// <summary>Orders and renders <see cref="ShellToolbarItem"/> registrations for both shells.</summary>
public static class ShellToolbarItems
{
    /// <summary>
    /// Orders <paramref name="items"/> by <see cref="ShellToolbarItem.Order"/>, keeping registration order when the orders match.
    /// </summary>
    /// <param name="items">The registrations. An empty sequence yields an empty list.</param>
    /// <returns>The items the top bar should draw, from the theme toggle outward.</returns>
    public static IReadOnlyList<ShellToolbarItem> InOrder(IEnumerable<ShellToolbarItem> items)
    {
        return items
            .Select((item, index) => (item, index))
            .OrderBy(pair => pair.item.Order)
            .ThenBy(pair => pair.index)
            .Select(pair => pair.item)
            .ToArray();
    }

    /// <summary>
    /// Draws <paramref name="items"/> and then <paramref name="trailing"/>, which is the host's
    /// <see cref="ForgeShellOptions.TopbarEndComponent"/>. Returns <see langword="null"/> when both are empty,
    /// so the top bar adds no extra markup.
    /// </summary>
    /// <param name="items">Toolbar components, already ordered.</param>
    /// <param name="trailing">A single host component drawn after the icons. <see langword="null"/> draws none.</param>
    /// <returns>The fragment for the top bar's end slot, or <see langword="null"/>.</returns>
    public static RenderFragment? Render(IReadOnlyList<ShellToolbarItem> items, Type? trailing)
    {
        if (items.Count == 0 && trailing is null)
        {
            return null;
        }

        return builder =>
        {
            int sequence = 0;
            foreach (ShellToolbarItem item in items)
            {
                builder.OpenComponent<DynamicComponent>(sequence++);
                builder.AddComponentParameter(sequence++, nameof(DynamicComponent.Type), item.ComponentType);
                builder.CloseComponent();
            }

            if (trailing is not null)
            {
                builder.OpenComponent<DynamicComponent>(sequence++);
                builder.AddComponentParameter(sequence++, nameof(DynamicComponent.Type), trailing);
                builder.CloseComponent();
            }
        };
    }
}
