using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Forms;

/// <summary>Search box with the magnifying glass. Updates on each keystroke.</summary>
public partial class SearchField
{
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? LabelClass { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? InputClass { get; set; }

    [Parameter]
    public string? GroupClass { get; set; }

    [Parameter]
    public string? IconClass { get; set; }

    [Parameter]
    public bool Small { get; set; }

    [Parameter]
    public string Autocomplete { get; set; } = "off";

    [Parameter]
    public string? SubmitText { get; set; }

    [Parameter]
    public EventCallback OnSubmit { get; set; }

    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    [Parameter]
    public RenderFragment? Trailing { get; set; }

    private string AriaLabelText => AriaLabel ?? Placeholder ?? "Search";

    private string ResolvedLabelClass => string.IsNullOrEmpty(LabelClass) ? "form-label" : LabelClass;

    private Task OnInputAsync(ChangeEventArgs args) => ValueChanged.InvokeAsync(args.Value?.ToString());

    private Task SubmitAsync() => OnSubmit.InvokeAsync();
}
