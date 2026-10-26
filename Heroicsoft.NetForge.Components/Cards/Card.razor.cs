using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Cards;

/// <summary>One tile: a title, a short description, and a meta row.</summary>
public partial class Card
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public RenderFragment? TitleEnd { get; set; }

    [Parameter]
    public RenderFragment? Meta { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }
}
