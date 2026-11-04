using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Menus;

/// <summary>One command in an <see cref="ActionMenu"/>: a link when <see cref="Href"/> is set, otherwise a button.</summary>
public partial class ActionMenuItem
{
    [CascadingParameter]
    private ActionMenu? Parent { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? Target { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <summary>Draws the item in the danger tone, for deletes and other destructive commands.</summary>
    [Parameter]
    public bool Danger { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Class { get; set; }

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
