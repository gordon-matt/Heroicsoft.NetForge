using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Chips;

/// <summary>A row of chips used as filters.</summary>
public partial class ChipGroup
{
    /// <summary>
    /// Chips or other markup in the row. <see langword="null"/> leaves the row empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra CSS classes on the <c>d-flex flex-wrap gap-2</c> row. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Inline style on the row. <see langword="null"/> sets none.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// ARIA role on the row. <see langword="null"/> omits the attribute.
    /// </summary>
    [Parameter]
    public string? Role { get; set; }
}