using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Forms;

/// <summary>A label with the control, hint, and validation message under it.</summary>
public partial class Field
{
    /// <summary>
    /// Text of the label. Ignored when <see cref="LabelContent"/> is set. <see langword="null"/> or empty hides the label.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Markup for the label. When set, it replaces <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    /// <summary>
    /// <c>for</c> on the label, matching the control <c>id</c>. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? For { get; set; }

    /// <summary>
    /// Classes on the label. <see langword="null"/> omits the attribute.
    /// </summary>
    [Parameter]
    public string? LabelClass { get; set; }

    /// <summary>
    /// Extra classes on the field, after <c>nf-field</c>. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// The control and anything under the label. <see langword="null"/> renders only the label.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}