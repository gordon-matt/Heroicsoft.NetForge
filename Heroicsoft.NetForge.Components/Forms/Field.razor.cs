using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Forms;

/// <summary>A label with the control, hint, and validation message under it.</summary>
public partial class Field
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    [Parameter]
    public string? For { get; set; }

    [Parameter]
    public string? LabelClass { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
