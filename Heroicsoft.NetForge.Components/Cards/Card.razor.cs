using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Cards;

/// <summary>One tile: a title, a short description, and a meta row.</summary>
public partial class Card
{
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the href.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the title end.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleEnd { get; set; }

    /// <summary>
    /// Gets or sets the meta.
    /// </summary>
    [Parameter]
    public RenderFragment? Meta { get; set; }

    /// <summary>
    /// Gets or sets the actions.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}