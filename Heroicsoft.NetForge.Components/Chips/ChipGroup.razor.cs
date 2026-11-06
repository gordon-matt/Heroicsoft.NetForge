using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Chips;

/// <summary>A row of chips used as filters.</summary>
public partial class ChipGroup
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

    /// <summary>
    /// Gets or sets the style.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    [Parameter]
    public string? Role { get; set; }
}