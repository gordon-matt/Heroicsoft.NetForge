using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Cards;

/// <summary>Wraps cards so they fill the row and wrap onto the next one.</summary>
public partial class CardGrid
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }
}
