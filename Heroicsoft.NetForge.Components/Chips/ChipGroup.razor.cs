using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Chips;

/// <summary>A row of chips used as filters.</summary>
public partial class ChipGroup
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public string? Role { get; set; }
}
