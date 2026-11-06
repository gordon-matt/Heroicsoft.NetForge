namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>What <see cref="TreeToggle"/> needs, without the item type.</summary>
public interface ITreeTableRow
{
    /// <summary>
    /// Gets the depth.
    /// </summary>
    int Depth { get; }

    /// <summary>
    /// Gets a value indicating whether this instance has children.
    /// </summary>
    bool HasChildren { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is expanded.
    /// </summary>
    bool IsExpanded { get; }

    /// <summary>
    /// Toggles the value.
    /// </summary>
    void Toggle();
}

/// <summary>One visible row of a <see cref="TreeTable{TItem}"/>.</summary>
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

    /// <summary>
    /// Gets the item.
    /// </summary>
    public TItem Item { get; }

    /// <summary>
    /// Gets the depth.
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Gets a value indicating whether this instance has children.
    /// </summary>
    public bool HasChildren { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is expanded.
    /// </summary>
    public bool IsExpanded { get; }

    /// <summary>
    /// Toggles the value.
    /// </summary>
    public void Toggle() => owner.Toggle(Item);
}
