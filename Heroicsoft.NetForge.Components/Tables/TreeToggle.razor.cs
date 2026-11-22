using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>The indent and expand button for the first cell of a <see cref="TreeTable{TItem}"/> row.</summary>
public partial class TreeToggle
{
    /// <summary>
    /// The row this cell belongs to. Required. Supplies the depth, whether there are children, and the toggle.
    /// </summary>
    [Parameter, EditorRequired]
    public ITreeTableRow Row { get; set; } = default!;

    /// <summary>
    /// Text or markup after the expander. <see langword="null"/> leaves that span empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
