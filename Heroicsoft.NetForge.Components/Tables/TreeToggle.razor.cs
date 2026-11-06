using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>The indent and expand button for the first cell of a <see cref="TreeTable{TItem}"/> row.</summary>
public partial class TreeToggle
{
    /// <summary>
    /// Gets or sets the row.
    /// </summary>
    [Parameter, EditorRequired]
    public ITreeTableRow Row { get; set; } = default!;

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
