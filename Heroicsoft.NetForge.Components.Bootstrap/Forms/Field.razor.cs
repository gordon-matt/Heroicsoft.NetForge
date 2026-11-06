using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Forms;

/// <summary>A label with the control under it.</summary>
public partial class Field
{
    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the label content.
    /// </summary>
    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    /// <summary>
    /// Gets or sets the for.
    /// </summary>
    [Parameter]
    public string? For { get; set; }

    /// <summary>
    /// Gets or sets the label class.
    /// </summary>
    [Parameter]
    public string? LabelClass { get; set; }

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string ResolvedLabelClass => string.IsNullOrEmpty(LabelClass) ? "form-label" : LabelClass;
}