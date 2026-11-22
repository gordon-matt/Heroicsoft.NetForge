using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Cards;

/// <summary>One tile: a title, a short description, and a meta row.</summary>
public partial class Card
{
    /// <summary>
    /// Heading text. <see langword="null"/> or empty hides the heading unless <see cref="TitleEnd"/> is set.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// When set, the title is a link to this address. <see langword="null"/> leaves the title as text.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Paragraph under the title. <see langword="null"/> hides it.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Markup after the title, inside the heading. <see langword="null"/> renders nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleEnd { get; set; }

    /// <summary>
    /// Start of the meta row. The row is hidden when this and <see cref="Actions"/> are both <see langword="null"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? Meta { get; set; }

    /// <summary>
    /// End of the meta row. <see langword="null"/> hides the actions. The row still shows when <see cref="Meta"/> is set.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Markup between the description and the meta row. <see langword="null"/> renders nothing there.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra classes on the card. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}