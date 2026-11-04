namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>What <see cref="TreeToggle"/> needs, without the item type.</summary>
public interface ITreeTableRow
{
    int Depth { get; }

    bool HasChildren { get; }

    bool IsExpanded { get; }

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

    public TItem Item { get; }

    public int Depth { get; }

    public bool HasChildren { get; }

    public bool IsExpanded { get; }

    public void Toggle() => owner.Toggle(Item);
}
