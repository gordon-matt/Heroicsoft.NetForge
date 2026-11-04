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

    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<bool>>? ValueExpression { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Class { get; set; }

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
