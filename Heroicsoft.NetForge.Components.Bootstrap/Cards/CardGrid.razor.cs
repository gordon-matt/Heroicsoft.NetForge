using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Cards;

/// <summary>Wraps cards so they fill the row and wrap onto the next one.</summary>
public partial class CardGrid
{
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