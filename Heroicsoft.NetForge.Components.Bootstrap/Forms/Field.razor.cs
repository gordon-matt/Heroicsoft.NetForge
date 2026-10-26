using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Forms;

/// <summary>A label with the control under it.</summary>
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

    private string ResolvedLabelClass => string.IsNullOrEmpty(LabelClass) ? "form-label" : LabelClass;
}
