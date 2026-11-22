using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Cards;

/// <summary>One tile: a title, a short description, and a meta row.</summary>
public partial class Card
{
    /// <summary>
    /// Heading text. When <see cref="Href"/> is set, the text is a link; otherwise it is plain text.
    /// The heading row is omitted when this is empty and <see cref="TitleEnd"/> is not set.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Address of the title link. <see langword="null"/> renders <see cref="Title"/> as text.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Secondary paragraph under the title. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Markup after the title, inside the heading. <see langword="null"/> draws nothing there.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleEnd { get; set; }

    /// <summary>
    /// Markup at the start of the footer row. <see langword="null"/> leaves that side empty.
    /// The row is omitted when this and <see cref="Actions"/> are both not set.
    /// </summary>
    [Parameter]
    public RenderFragment? Meta { get; set; }

    /// <summary>
    /// Markup aligned to the end of the footer row. <see langword="null"/> omits that group.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Markup between the description and the footer row. <see langword="null"/> draws nothing there.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra CSS classes on the <c>card h-100</c> article. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}