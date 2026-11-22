using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Cards;

/// <summary>Wraps cards so they fill the row and wrap onto the next one.</summary>
public partial class CardGrid
{
    /// <summary>
    /// The cards. <see langword="null"/> renders an empty grid.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra classes on the grid. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}