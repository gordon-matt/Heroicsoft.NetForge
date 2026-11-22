using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Heroicsoft.NetForge.Components.Forms;

/// <summary>
/// An on/off toggle for settings that apply immediately or read as a state rather than a choice.
/// Works with <c>@bind-Value</c> inside or outside an <see cref="EditForm"/>.
/// </summary>
public partial class Switch
{
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    /// <summary>
    /// Whether the switch is on. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// Raised with the new checked state when the user toggles the switch. When <see cref="ValueExpression"/> is set inside an edit form, the field is also marked changed.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    /// <summary>
    /// Field used to notify the surrounding edit form. <see langword="null"/> skips that notification. <c>@bind-Value</c> sets this.
    /// </summary>
    [Parameter]
    public Expression<Func<bool>>? ValueExpression { get; set; }

    /// <summary>
    /// Text beside the switch. Ignored when <see cref="ChildContent"/> is set. <see langword="null"/> or empty hides the text unless a description or child content is set.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Hint under the label. <see langword="null"/> or empty hides it.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// <c>id</c> of the checkbox. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Disables the checkbox and marks the control disabled. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Extra classes on the switch. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Replaces <see cref="Label"/> as the text beside the switch. <see langword="null"/> uses <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private async Task OnChangeAsync(ChangeEventArgs args)
    {
        Value = args.Value is bool value ? value : bool.TryParse(args.Value?.ToString(), out bool parsed) && parsed;
        await ValueChanged.InvokeAsync(Value);

        if (EditContext is not null && ValueExpression is not null)
        {
            EditContext.NotifyFieldChanged(FieldIdentifier.Create(ValueExpression));
        }
    }
}
