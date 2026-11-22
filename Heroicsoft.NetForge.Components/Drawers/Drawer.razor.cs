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

    /// <summary>
    /// The scrolling body. <see langword="null"/> leaves the body empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A bar pinned under the scrolling body, for Save and Cancel on a long form. A submit button
    /// here reaches a form in the body through its <c>form</c> attribute.
    /// </summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// Raised with no argument when the close button is clicked, when Escape is pressed while the panel is open, and when the scrim is clicked.
    /// </summary>
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

    /// <summary>
    /// Edge the panel grows from. Defaults to <see cref="DrawerSide.End"/>, the right edge in left-to-right layout. <see cref="DrawerSide.Start"/> adds the start-edge style.
    /// </summary>
    [Parameter]
    public DrawerSide Side { get; set; } = DrawerSide.End;

    /// <summary>
    /// Extra classes on the panel. <see langword="null"/> or whitespace adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Extra classes on the scrolling body. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? BodyClass { get; set; }

    /// <summary>
    /// Accessible name of the panel. <see langword="null"/> or whitespace uses <see cref="Title"/>, then <c>Panel</c>.
    /// </summary>
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