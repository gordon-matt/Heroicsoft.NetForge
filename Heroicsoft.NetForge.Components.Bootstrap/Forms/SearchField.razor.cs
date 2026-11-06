using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Forms;

/// <summary>Search box with the magnifying glass. Updates on each keystroke.</summary>
public partial class SearchField
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the value changed.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the label class.
    /// </summary>
    [Parameter]
    public string? LabelClass { get; set; }

    /// <summary>
    /// Gets or sets the placeholder.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the aria label.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the input class.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Gets or sets the group class.
    /// </summary>
    [Parameter]
    public string? GroupClass { get; set; }

    /// <summary>
    /// Gets or sets the icon class.
    /// </summary>
    [Parameter]
    public string? IconClass { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is small.
    /// </summary>
    [Parameter]
    public bool Small { get; set; }

    /// <summary>
    /// Gets or sets the autocomplete.
    /// </summary>
    [Parameter]
    public string Autocomplete { get; set; } = "off";

    /// <summary>
    /// Gets or sets the submit text.
    /// </summary>
    [Parameter]
    public string? SubmitText { get; set; }

    /// <summary>
    /// Gets or sets the callback raised for the submit event.
    /// </summary>
    [Parameter]
    public EventCallback OnSubmit { get; set; }

    /// <summary>
    /// Gets or sets the callback raised for the key down event.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Gets or sets the callback raised for the key up event.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    /// <summary>
    /// Gets or sets the trailing.
    /// </summary>
    [Parameter]
    public RenderFragment? Trailing { get; set; }

    private string AriaLabelText => AriaLabel ?? Placeholder ?? "Search";

    private string ResolvedLabelClass => string.IsNullOrEmpty(LabelClass) ? "form-label" : LabelClass;

    private Task OnInputAsync(ChangeEventArgs args) => ValueChanged.InvokeAsync(args.Value?.ToString());

    private Task SubmitAsync() => OnSubmit.InvokeAsync();
}