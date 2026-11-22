using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Forms;

/// <summary>Search box with the magnifying glass. Updates on each keystroke.</summary>
public partial class SearchField
{
    /// <summary>
    /// Text currently shown in the search box. <see langword="null"/> shows an empty box.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Invoked with the box text on each keystroke. The argument is <see langword="null"/> when the input event has no value.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Text of a <c>form-label</c> above the box. <see langword="null"/> or empty omits the label.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// CSS classes on the label. <see langword="null"/> or empty uses <c>form-label</c>.
    /// </summary>
    [Parameter]
    public string? LabelClass { get; set; }

    /// <summary>
    /// Placeholder text. Also the accessible name when <see cref="AriaLabel"/> is <see langword="null"/>.
    /// When both are <see langword="null"/>, the accessible name is <c>Search</c>.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Accessible name of the input. <see langword="null"/> uses <see cref="Placeholder"/>, then <c>Search</c>.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Id of the input and the label's <c>for</c> value. <see langword="null"/> omits the id.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// CSS classes on the outer element. <see langword="null"/> adds none. The outer element is a
    /// form with <c>role="search"</c> when <see cref="OnSubmit"/> has a delegate, and a div otherwise.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Extra CSS classes on the <c>form-control</c> input. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Extra CSS classes on the <c>input-group</c>. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? GroupClass { get; set; }

    /// <summary>
    /// Extra CSS classes on the magnifying-glass <c>input-group-text</c>. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? IconClass { get; set; }

    /// <summary>
    /// When <see langword="true"/>, adds <c>input-group-sm</c>. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Small { get; set; }

    /// <summary>
    /// Value of the input <c>autocomplete</c> attribute. Defaults to <c>off</c>.
    /// </summary>
    [Parameter]
    public string Autocomplete { get; set; } = "off";

    /// <summary>
    /// Label of a <c>btn-primary</c> submit button after the input. <see langword="null"/> or empty omits the button.
    /// </summary>
    [Parameter]
    public string? SubmitText { get; set; }

    /// <summary>
    /// Invoked when the search form is submitted. When no delegate is set, the box is not wrapped in a form.
    /// </summary>
    [Parameter]
    public EventCallback OnSubmit { get; set; }

    /// <summary>
    /// Invoked for each keydown in the input.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    /// <summary>
    /// Invoked for each keyup in the input.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    /// <summary>
    /// Markup after the input and the submit button, inside the input group. <see langword="null"/> draws nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? Trailing { get; set; }

    private string AriaLabelText => AriaLabel ?? Placeholder ?? "Search";

    private string ResolvedLabelClass => string.IsNullOrEmpty(LabelClass) ? "form-label" : LabelClass;

    private Task OnInputAsync(ChangeEventArgs args) => ValueChanged.InvokeAsync(args.Value?.ToString());

    private Task SubmitAsync() => OnSubmit.InvokeAsync();
}