using Heroicsoft.NetForge.Components.Paging;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Paging;

/// <summary>
/// Renders a result range, and when there is more than one page a small pagination bar: first, previous,
/// up to five page numbers, next, and last. A page-size list appears when <see cref="PageSizeChanged"/> has a
/// handler. A non-positive <see cref="TotalCount"/> renders nothing.
/// </summary>
public partial class Pager
{
    private readonly string pageSizeId = "bf-pager-" + Guid.NewGuid().ToString("N");

    private static readonly int[] DefaultPageSizes = [12, 24, 48, 96];

    /// <summary>
    /// The current page, starting at 1. That button uses <c>active</c>. First and previous are
    /// disabled on page 1; next and last are disabled on the final page. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// How many results one page holds. Used for the range text and the selected size. The property
    /// initializer is 24. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public int PageSize { get; set; } = 24;

    /// <summary>
    /// How many results exist. A positive count shows the first and last index on this page and the
    /// total. A non-positive count renders nothing. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public int TotalCount { get; set; }

    /// <summary>
    /// Invoked with the chosen page when a page button is used and that page is not already current.
    /// The page is clamped to the pages that exist.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    /// <summary>
    /// Invoked with the newly selected page size. When no handler is set, the per-page list is hidden.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    /// <summary>
    /// Options in the per-page list. <see langword="null"/> or empty uses 12, 24, 48, and 96.
    /// </summary>
    [Parameter]
    public IReadOnlyList<int>? PageSizes { get; set; }

    /// <summary>
    /// Accessible name of the pagination <c>nav</c>. Defaults to <c>Pagination</c>.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Pagination";

    private IReadOnlyList<int> Sizes => PageSizes is { Count: > 0 } ? PageSizes : DefaultPageSizes;

    private int PageCount => PagerMath.PageCount(TotalCount, PageSize);

    private int FirstOnPage => PagerMath.FirstOnPage(PageNumber, PageSize);

    private int LastOnPage => PagerMath.LastOnPage(PageNumber, PageSize, TotalCount);

    private IReadOnlyList<int> VisiblePages => PagerMath.VisiblePages(PageNumber, PageCount);

    private async Task GoAsync(int page)
    {
        page = Math.Clamp(page, 1, PageCount);

        if (page != PageNumber)
        {
            await PageNumberChanged.InvokeAsync(page);
        }
    }

    private async Task OnPageSizeChangedAsync(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out int size))
        {
            await PageSizeChanged.InvokeAsync(size);
        }
    }
}