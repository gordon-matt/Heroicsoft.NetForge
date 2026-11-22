using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Forms;

/// <summary>A label with the control under it.</summary>
public partial class Field
{
    /// <summary>
    /// Label text. Ignored when <see cref="LabelContent"/> is set. <see langword="null"/> or empty
    /// omits the label unless <see cref="LabelContent"/> is set.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Markup for the label. When set, replaces <see cref="Label"/>. <see langword="null"/> uses <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    /// <summary>
    /// Id of the control this label points at, written to the label's <c>for</c> attribute.
    /// <see langword="null"/> omits the attribute.
    /// </summary>
    [Parameter]
    public string? For { get; set; }

    /// <summary>
    /// CSS classes on the label. <see langword="null"/> or empty uses <c>form-label</c>.
    /// </summary>
    [Parameter]
    public string? LabelClass { get; set; }

    /// <summary>
    /// CSS classes on the wrapper. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// The control rendered under the label. <see langword="null"/> renders only the label.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string ResolvedLabelClass => string.IsNullOrEmpty(LabelClass) ? "form-label" : LabelClass;
}