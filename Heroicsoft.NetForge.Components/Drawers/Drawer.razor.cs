using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Drawers;

/// <summary>A side panel with a title, optional actions, and a close button.</summary>
public partial class Drawer
{
    /// <summary>Shown when <see cref="TitleContent"/> is not set.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>Replaces <see cref="Title"/> when the heading is more than text.</summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>Buttons placed before the close button.</summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>Dims the page behind a fixed panel. Inline panels ignore this.</summary>
    [Parameter]
    public bool Scrim { get; set; }

    /// <summary>Position the panel inside its parent instead of the viewport.</summary>
    [Parameter]
    public bool Inline { get; set; }

    /// <summary>When false, a non-persistent panel is not rendered.</summary>
    [Parameter]
    public bool Open { get; set; } = true;

    /// <summary>Keep the panel mounted while closed so it can slide in.</summary>
    [Parameter]
    public bool Persistent { get; set; }

    [Parameter]
    public DrawerSide Side { get; set; } = DrawerSide.End;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? BodyClass { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>CSS width. The stylesheet supplies a default when this is empty.</summary>
    [Parameter]
    public string? Width { get; set; }

    private string Label => string.IsNullOrWhiteSpace(AriaLabel) ? Title ?? "Panel" : AriaLabel;

    private string? PanelStyle => string.IsNullOrWhiteSpace(Width) ? null : $"width: {Width}";

    private string PanelClass
    {
        get
        {
            var classes = new List<string> { "nf-drawer" };
            if (Inline)
            {
                classes.Add("is-inline");
            }

            if (Side == DrawerSide.Start)
            {
                classes.Add("is-start");
            }

            if (Persistent)
            {
                classes.Add("is-persistent");
            }

            if (Persistent && !Open)
            {
                classes.Add("is-closed");
            }

            if (!string.IsNullOrWhiteSpace(Class))
            {
                classes.Add(Class);
            }

            return string.Join(' ', classes);
        }
    }

    private Task CloseAsync() => OnClose.InvokeAsync();

    private Task OnKeyDownAsync(KeyboardEventArgs args) =>
        Open && args.Key == "Escape" ? OnClose.InvokeAsync() : Task.CompletedTask;
}

/// <summary>Which edge a <see cref="Drawer"/> grows from.</summary>
public enum DrawerSide
{
    /// <summary>The left edge in left-to-right layout.</summary>
    Start,

    /// <summary>The right edge in left-to-right layout.</summary>
    End,
}
