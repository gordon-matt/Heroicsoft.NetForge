using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Menus;

/// <summary>
/// A button that opens a short list of commands, typically the row actions of a table. The list is
/// positioned against the viewport so a scrolling table wrapper does not clip it.
/// </summary>
public partial class ActionMenu
{
    private ElementReference trigger;
    private ElementReference menu;
    private bool open;
    private bool placePending;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    /// <summary>Accessible name and tooltip of the trigger button.</summary>
    [Parameter]
    public string Label { get; set; } = "Actions";

    /// <summary>
    /// Icon on the trigger when <see cref="Trigger"/> is <see langword="null"/>. Defaults to <c>dots</c>.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "dots";

    /// <summary>Replaces the icon inside the trigger, for example with text and a chevron.</summary>
    [Parameter]
    public RenderFragment? Trigger { get; set; }

    /// <summary>Classes for the trigger button. Defaults to <c>nf-icon-button</c>.</summary>
    [Parameter]
    public string? TriggerClass { get; set; }

    /// <summary>
    /// Disables the trigger. Defaults to <see langword="false"/>. Does not close a list that is already open.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Extra classes on the menu wrapper. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// The commands, rendered only while the list is open. <see langword="null"/> opens an empty list.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// <see langword="true"/> while the command list is showing.
    /// </summary>
    public bool IsOpen => open;

    /// <summary>
    /// Hides the command list. Does nothing when it is already hidden.
    /// </summary>
    public void Close()
    {
        if (open)
        {
            open = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Positions the open list against the viewport. If the page script has no <c>placeMenu</c>, the list stays under the button.
    /// </summary>
    /// <param name="firstRender">Ignored. Placement runs after the list opens, not only on the first render.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!placePending)
        {
            return;
        }

        placePending = false;
        try
        {
            await JS.InvokeVoidAsync("netForge.placeMenu", trigger, menu);
        }
        catch (JSException)
        {
            // An older nf.js without placeMenu: the CSS fallback opens the list under the button.
        }
    }

    private void Toggle()
    {
        open = !open;
        placePending = open;
    }

    private void OnKeyDown(KeyboardEventArgs args)
    {
        if (open && args.Key == "Escape")
        {
            open = false;
        }
    }
}
