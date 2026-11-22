using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>A clickable column header for a <see cref="Table"/>. Bind it with <c>@bind-Sort</c>.</summary>
public partial class SortHeader
{
    /// <summary>
    /// Column name passed to <see cref="TableSort.Toggle"/>. Required. Compared ordinally with <see cref="TableSort.Field"/> to choose the arrow. The default is empty.
    /// </summary>
    [Parameter, EditorRequired]
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// The table's current sort. <see langword="null"/> leaves this column unsorted. When <see cref="Field"/> matches, the header shows the direction.
    /// </summary>
    [Parameter]
    public TableSort? Sort { get; set; }

    /// <summary>
    /// Raised with the next sort when the header is clicked. The same column flips direction. A different column, or a <see langword="null"/> sort, starts ascending.
    /// </summary>
    [Parameter]
    public EventCallback<TableSort?> SortChanged { get; set; }

    /// <summary>
    /// The header label. <see langword="null"/> leaves the button text empty.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra classes on the header cell. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Inline style on the header cell. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    private bool IsSorted => Sort is not null && string.Equals(Sort.Field, Field, StringComparison.Ordinal);

    private string StateClass => !IsSorted ? string.Empty : Sort!.Descending ? "is-desc" : "is-asc";

    private string AriaSort => !IsSorted ? "none" : Sort!.Descending ? "descending" : "ascending";

    private Task ToggleAsync() => SortChanged.InvokeAsync(TableSort.Toggle(Sort, Field));
}
