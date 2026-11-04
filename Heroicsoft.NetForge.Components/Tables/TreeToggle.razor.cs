using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>The indent and expand button for the first cell of a <see cref="TreeTable{TItem}"/> row.</summary>
public partial class TreeToggle
{
    [Parameter, EditorRequired]
    public ITreeTableRow Row { get; set; } = default!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
