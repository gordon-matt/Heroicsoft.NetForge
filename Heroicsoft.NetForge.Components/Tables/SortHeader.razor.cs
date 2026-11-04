using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>A clickable column header for a <see cref="Table"/>. Bind it with <c>@bind-Sort</c>.</summary>
public partial class SortHeader
{
    [Parameter, EditorRequired]
    public string Field { get; set; } = string.Empty;

    [Parameter]
    public TableSort? Sort { get; set; }

    [Parameter]
    public EventCallback<TableSort?> SortChanged { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    private bool IsSorted => Sort is not null && string.Equals(Sort.Field, Field, StringComparison.Ordinal);

    private string StateClass => !IsSorted ? string.Empty : Sort!.Descending ? "is-desc" : "is-asc";

    private string AriaSort => !IsSorted ? "none" : Sort!.Descending ? "descending" : "ascending";

    private Task ToggleAsync() => SortChanged.InvokeAsync(TableSort.Toggle(Sort, Field));
}
