using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Cards;

/// <summary>Wraps cards so they fill the row and wrap onto the next one.</summary>
public partial class CardGrid
{
    /// <summary>
    /// Cards or other markup inside the <c>bf-card-grid</c> wrapper. <see langword="null"/> leaves the grid empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra CSS classes on the grid wrapper. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}