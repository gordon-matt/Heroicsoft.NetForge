using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Menus;

/// <summary>One command in an <see cref="ActionMenu"/>: a link when <see cref="Href"/> is set, otherwise a button.</summary>
public partial class ActionMenuItem
{
    [CascadingParameter]
    private ActionMenu? Parent { get; set; }

    /// <summary>
    /// Label used when <see cref="ChildContent"/> is <see langword="null"/>. <see langword="null"/> leaves the label empty.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Icon before the label. <see langword="null"/> or empty hides it.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// When set, and the item is not disabled, the item is a link to this address and a click closes the menu. <see langword="null"/>, empty, or <see cref="Disabled"/> renders a button.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// <c>target</c> of the link, such as <c>_blank</c>. <see langword="null"/> omits it. Ignored when the item is a button.
    /// </summary>
    [Parameter]
    public string? Target { get; set; }

    /// <summary>
    /// Raised with no argument when the button is clicked, after the menu closes. A link does not raise it.
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <summary>Draws the item in the danger tone, for deletes and other destructive commands.</summary>
    [Parameter]
    public bool Danger { get; set; }

    /// <summary>
    /// Forces a disabled button even when <see cref="Href"/> is set. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Extra classes on the item. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Replaces <see cref="Text"/>. <see langword="null"/> uses <see cref="Text"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Extra attributes, for example <c>download</c> on a file link.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private async Task ClickAsync()
    {
        // Close first so a confirm dialog opened by the handler is not covered by the menu.
        Parent?.Close();
        await OnClick.InvokeAsync();
    }
}
