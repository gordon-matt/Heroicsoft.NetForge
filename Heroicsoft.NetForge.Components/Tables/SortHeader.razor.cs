using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>A clickable column header for a <see cref="Table"/>. Bind it with <c>@bind-Sort</c>.</summary>
public partial class SortHeader
{
    /// <summary>
    /// Gets or sets the field.
    /// </summary>
    [Parameter, EditorRequired]
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sort.
    /// </summary>
    [Parameter]
    public TableSort? Sort { get; set; }

    /// <summary>
    /// Gets or sets the sort changed.
    /// </summary>
    [Parameter]
    public EventCallback<TableSort?> SortChanged { get; set; }

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

    private bool IsSorted => Sort is not null && string.Equals(Sort.Field, Field, StringComparison.Ordinal);

    private string StateClass => !IsSorted ? string.Empty : Sort!.Descending ? "is-desc" : "is-asc";

    private string AriaSort => !IsSorted ? "none" : Sort!.Descending ? "descending" : "ascending";

    private Task ToggleAsync() => SortChanged.InvokeAsync(TableSort.Toggle(Sort, Field));
}
