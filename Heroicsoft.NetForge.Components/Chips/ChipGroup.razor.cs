using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Chips;

/// <summary>A row of chips used as filters.</summary>
public partial class ChipGroup
{
    /// <summary>
    /// The chips. <see langword="null"/> renders an empty row.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra classes on the row. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Inline style on the row. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// ARIA role of the row. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Role { get; set; }
}