using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Forms;

/// <summary>Search box with the magnifying glass. Updates on each keystroke.</summary>
public partial class SearchField
{
    /// <summary>
    /// Text in the box. <see langword="null"/> shows an empty field.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Raised on each keystroke with the field text. The argument is <see langword="null"/> when the input event has no value.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Placeholder text. Also the accessible name when <see cref="AriaLabel"/> is <see langword="null"/>.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Accessible name of the field. <see langword="null"/> uses <see cref="Placeholder"/>, then <c>Search</c>.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// <c>id</c> of the input. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Extra classes on the search wrapper. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Classes on the input. <see langword="null"/> omits them.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// <c>autocomplete</c> on the input. Defaults to <c>off</c>.
    /// </summary>
    [Parameter]
    public string Autocomplete { get; set; } = "off";

    /// <summary>
    /// Label of the button after the field. <see langword="null"/> or empty hides the button.
    /// </summary>
    [Parameter]
    public string? SubmitText { get; set; }

    /// <summary>
    /// Raised with no argument when the form is submitted. A delegate makes the control a form and cancels the browser submit. Without a delegate the control is a label and the button does not submit.
    /// </summary>
    [Parameter]
    public EventCallback OnSubmit { get; set; }

    /// <summary>
    /// Raised with the key event when a key goes down in the field.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Raised with the key event when a key goes up in the field.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    /// <summary>
    /// Markup after the input, such as a shortcut hint. <see langword="null"/> renders nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? Trailing { get; set; }

    /// <summary>Marks the input so the slash key focuses it. Defaults to <see langword="false"/>.</summary>
    [Parameter]
    public bool Shortcut { get; set; }

    private string AriaLabelText => AriaLabel ?? Placeholder ?? "Search";

    private Task OnInputAsync(ChangeEventArgs args) => ValueChanged.InvokeAsync(args.Value?.ToString());

    private Task SubmitAsync() => OnSubmit.InvokeAsync();
}