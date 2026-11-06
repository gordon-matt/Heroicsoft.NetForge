using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>
/// A table of nested rows, for page trees, menus and categories. The row template draws a whole
/// <c>&lt;tr&gt;</c>; put <see cref="TreeToggle"/> in the first cell for the indent and expander.
/// </summary>
public partial class TreeTable<TItem>
{
    // Keys whose state differs from ExpandedByDefault, so new rows follow the default.
    private readonly HashSet<object> toggled = [];
    private List<TreeTableRow<TItem>> rows = [];

    /// <summary>The top-level items.</summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the children of.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TItem, IEnumerable<TItem>> ChildrenOf { get; set; } = _ => [];

    /// <summary>A stable key per item, so expansion survives a reload of the items.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, object> KeyOf { get; set; } = item => item!;

    /// <summary>
    /// Gets or sets the row template.
    /// </summary>
    [Parameter, EditorRequired]
    public RenderFragment<TreeTableRow<TItem>> RowTemplate { get; set; } = default!;

    /// <summary>The header row, including its <c>&lt;tr&gt;</c>.</summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is expanded by default.
    /// </summary>
    [Parameter]
    public bool ExpandedByDefault { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is framed.
    /// </summary>
    [Parameter]
    public bool Framed { get; set; } = true;

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>The rows currently shown, in display order.</summary>
    public IReadOnlyList<TreeTableRow<TItem>> VisibleRows => rows;

    /// <inheritdoc/>
    protected override void OnParametersSet() => Build();

    /// <summary>
    /// Performs the expand all operation.
    /// </summary>
    public void ExpandAll() => SetAll(expanded: true);

    /// <summary>
    /// Performs the collapse all operation.
    /// </summary>
    public void CollapseAll() => SetAll(expanded: false);

    internal void Toggle(TItem item)
    {
        object key = KeyOf(item);
        if (!toggled.Remove(key))
        {
            toggled.Add(key);
        }

        Build();
        StateHasChanged();
    }

    private void SetAll(bool expanded)
    {
        toggled.Clear();
        if (expanded != ExpandedByDefault)
        {
            var seen = new HashSet<object>();
            CollectKeys(Items, seen);
            toggled.UnionWith(seen);
        }

        Build();
        StateHasChanged();
    }

    private void CollectKeys(IEnumerable<TItem> items, HashSet<object> seen)
    {
        foreach (var item in items)
        {
            if (seen.Add(KeyOf(item)))
            {
                CollectKeys(ChildrenOf(item), seen);
            }
        }
    }

    private void Build()
    {
        rows = [];
        Add(Items, 0, []);
    }

    private void Add(IEnumerable<TItem> items, int depth, HashSet<object> path)
    {
        foreach (var item in items)
        {
            object key = KeyOf(item);

            // Bad data with a cycle would otherwise recurse forever.
            if (!path.Add(key))
            {
                continue;
            }

            var children = ChildrenOf(item).ToList();
            bool expanded = children.Count > 0 && ExpandedByDefault != toggled.Contains(key);
            rows.Add(new TreeTableRow<TItem>(this, item, depth, children.Count > 0, expanded));

            if (expanded)
            {
                Add(children, depth + 1, path);
            }

            path.Remove(key);
        }
    }
}
