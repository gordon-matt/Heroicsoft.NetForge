namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>
/// A labelled group of links. A null heading renders the items with no label.
/// In the top bar, a heading becomes a menu that holds the items.
/// </summary>
public sealed class ForgeNavSection
{
    public string? Heading { get; init; }

    public string? Roles { get; init; }

    /// <summary>
    /// Null follows the menu. True draws this group in the top bar. False draws it in the sidebar.
    /// </summary>
    public bool? InTopbar { get; init; }

    public IReadOnlyList<ForgeNavItem> Items { get; init; } = [];
}