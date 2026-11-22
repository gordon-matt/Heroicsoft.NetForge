namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>What <see cref="TreeToggle"/> needs, without the item type.</summary>
public interface ITreeTableRow
{
    /// <summary>Zero-based indent of the row. The toggle uses it as the depth offset.</summary>
    int Depth { get; }

    /// <summary>
    /// <see langword="true"/> when the item has at least one child. <see langword="false"/> shows a spacer instead of the expand button.
    /// </summary>
    bool HasChildren { get; }

    /// <summary>
    /// <see langword="true"/> when this row's children are included in the table.
    /// </summary>
    bool IsExpanded { get; }

    /// <summary>
    /// Expands or collapses this row and rebuilds the visible rows.
    /// </summary>
    void Toggle();
}

/// <summary>One visible row of a <see cref="TreeTable{TItem}"/>.</summary>
/// <typeparam name="TItem">The data for this row.</typeparam>
public sealed class TreeTableRow<TItem> : ITreeTableRow
{
    private readonly TreeTable<TItem> owner;

    internal TreeTableRow(TreeTable<TItem> owner, TItem item, int depth, bool hasChildren, bool isExpanded)
    {
        this.owner = owner;
        Item = item;
        Depth = depth;
        HasChildren = hasChildren;
        IsExpanded = isExpanded;
    }

    /// <summary>The data this row displays.</summary>
    public TItem Item { get; }

    /// <inheritdoc/>
    public int Depth { get; }

    /// <inheritdoc/>
    public bool HasChildren { get; }

    /// <inheritdoc/>
    public bool IsExpanded { get; }

    /// <inheritdoc/>
    public void Toggle() => owner.Toggle(Item);
}
