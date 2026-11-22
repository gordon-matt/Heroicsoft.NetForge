using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Drawers;

/// <summary>A side panel with a title, optional actions, and a close button.</summary>
public partial class Drawer
{
    /// <summary>
    /// Heading text, shown in bold when <see cref="TitleContent"/> is not set. <see langword="null"/> or
    /// empty omits that text. Also the accessible name when <see cref="AriaLabel"/> is blank.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Replaces <see cref="Title"/> when the heading is more than text. <see langword="null"/> uses <see cref="Title"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// Buttons placed before the close button. <see langword="null"/> draws none.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Markup in the panel body. <see langword="null"/> leaves the body empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Invoked from the close button, from Escape while the panel is open, and from a click on the scrim.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// When <see langword="true"/>, and the panel is open and not inline, dims the page behind the panel.
    /// A click on that scrim raises <see cref="OnClose"/>. Inline panels ignore this. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Scrim { get; set; }

    /// <summary>
    /// Position the panel inside its parent instead of the viewport. Adds <c>is-inline</c> and skips the scrim.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Inline { get; set; }

    /// <summary>
    /// When <see langword="false"/>, a non-persistent panel is not rendered. A <see cref="Persistent"/> panel
    /// stays mounted and gains <c>is-closed</c>. Defaults to <see langword="true"/>. Escape raises
    /// <see cref="OnClose"/> only while this is <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool Open { get; set; } = true;

    /// <summary>
    /// Keep the panel mounted while closed so it can slide in. Adds <c>is-persistent</c>.
    /// Defaults to <see langword="false"/>, which removes the panel when <see cref="Open"/> is <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Persistent { get; set; }

    /// <summary>
    /// Edge the panel grows from. <see cref="DrawerSide.End"/> is the default and adds no edge class.
    /// <see cref="DrawerSide.Start"/> adds <c>is-start</c>.
    /// </summary>
    [Parameter]
    public DrawerSide Side { get; set; } = DrawerSide.End;

    /// <summary>
    /// Extra CSS classes on the <c>bf-drawer</c> panel. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Extra CSS classes on the <c>bf-drawer-body</c> region. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? BodyClass { get; set; }

    /// <summary>
    /// Accessible name of the panel. <see langword="null"/> or blank uses <see cref="Title"/>, or <c>Panel</c> when that is also blank.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// CSS width. The stylesheet supplies a default when this is <see langword="null"/> or blank.
    /// Any other value is applied as an inline <c>width</c>.
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    private string Label => string.IsNullOrWhiteSpace(AriaLabel) ? Title ?? "Panel" : AriaLabel;

    private string? PanelStyle => string.IsNullOrWhiteSpace(Width) ? null : $"width: {Width}";

    private string PanelClass
    {
        get
        {
            var classes = new List<string> { "bf-drawer" };
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