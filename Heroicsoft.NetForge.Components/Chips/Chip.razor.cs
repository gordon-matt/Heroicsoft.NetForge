using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Chips;

/// <summary>One filter chip. Renders a link when <see cref="Href"/> is set, otherwise a button.</summary>
public partial class Chip
{
    /// <summary>
    /// When set, the chip is a link to this address. <see langword="null"/> renders a button.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Adds the active style. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Selected { get; set; }

    /// <summary>
    /// Raised with the click when the chip is activated, whether it is a link or a button.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// The chip label. <see langword="null"/> renders an empty chip.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra classes after the chip class and the active class. <see langword="null"/> or whitespace adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    private string CssClass
    {
        get
        {
            var classes = new List<string> { "nf-chip" };
            if (Selected)
            {
                classes.Add("is-active");
            }

            if (!string.IsNullOrWhiteSpace(Class))
            {
                classes.Add(Class);
            }

            return string.Join(' ', classes);
        }
    }
}