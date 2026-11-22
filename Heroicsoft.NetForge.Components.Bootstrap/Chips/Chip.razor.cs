using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Chips;

/// <summary>One filter chip. Renders a link when <see cref="Href"/> is set, otherwise a button.</summary>
public partial class Chip
{
    /// <summary>
    /// When set, the chip is an anchor to this address. <see langword="null"/> renders a button instead.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the chip uses <c>btn-primary</c>. When <see langword="false"/>,
    /// the default, it uses <c>btn-outline-secondary</c>.
    /// </summary>
    [Parameter]
    public bool Selected { get; set; }

    /// <summary>
    /// Invoked when the chip is clicked. A link chip still navigates to <see cref="Href"/>.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Label inside the chip. <see langword="null"/> leaves the chip empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra CSS classes after <c>btn btn-sm</c> and the selected or outline class. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    private string CssClass
    {
        get
        {
            var classes = new List<string>
            {
                "btn",
                "btn-sm",
                Selected ? "btn-primary" : "btn-outline-secondary"
            };
            if (!string.IsNullOrWhiteSpace(Class))
            {
                classes.Add(Class);
            }

            return string.Join(' ', classes);
        }
    }
}