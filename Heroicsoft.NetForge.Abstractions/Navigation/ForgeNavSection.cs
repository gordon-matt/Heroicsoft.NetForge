namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>
/// A labelled group of links. A <see langword="null"/> heading renders the items with no label.
/// In the top bar, a heading becomes a menu that holds the items.
/// </summary>
public sealed class ForgeNavSection
{
    /// <summary>
    /// Label for the group. A <see langword="null"/> heading renders the items with no label.
    /// In the top bar, a heading becomes a menu that holds the items.
    /// </summary>
    public string? Heading { get; init; }

    /// <summary>
    /// Comma-separated roles that can see the group. Empty means the group is shown to everyone,
    /// including anonymous visitors.
    /// </summary>
    public string? Roles { get; init; }

    /// <summary>
    /// <see langword="null"/> follows the menu. <see langword="true"/> draws this group in the top bar.
    /// <see langword="false"/> draws it in the sidebar.
    /// </summary>
    public bool? InTopbar { get; init; }

    /// <summary>Links in this group. Defaults to an empty list.</summary>
    public IReadOnlyList<ForgeNavItem> Items { get; init; } = [];
}