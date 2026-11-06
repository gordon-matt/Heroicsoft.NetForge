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
    /// Gets or sets a value indicating whether this instance is value.
    /// </summary>
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// Gets or sets the value changed.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the value expression.
    /// </summary>
    [Parameter]
    public Expression<Func<bool>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

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
