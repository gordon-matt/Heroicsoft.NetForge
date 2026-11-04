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

    [Parameter]
    public string Icon { get; set; } = "dots";

    /// <summary>Replaces the icon inside the trigger, for example with text and a chevron.</summary>
    [Parameter]
    public RenderFragment? Trigger { get; set; }

    /// <summary>Classes for the trigger button. Defaults to <c>nf-icon-button</c>.</summary>
    [Parameter]
    public string? TriggerClass { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public bool IsOpen => open;

    public void Close()
    {
        if (open)
        {
            open = false;
            StateHasChanged();
        }
    }

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
