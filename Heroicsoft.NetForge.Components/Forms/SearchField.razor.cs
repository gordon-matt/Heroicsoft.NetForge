using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Forms;

/// <summary>Search box with the magnifying glass. Updates on each keystroke.</summary>
public partial class SearchField
{
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

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

    /// <summary>Marks the input so the slash key focuses it.</summary>
    [Parameter]
    public bool Shortcut { get; set; }

    private string AriaLabelText => AriaLabel ?? Placeholder ?? "Search";

    private Task OnInputAsync(ChangeEventArgs args) => ValueChanged.InvokeAsync(args.Value?.ToString());

    private Task SubmitAsync() => OnSubmit.InvokeAsync();
}
