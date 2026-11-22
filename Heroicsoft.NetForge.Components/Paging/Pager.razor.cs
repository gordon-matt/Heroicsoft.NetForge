using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Paging;

/// <summary>
/// Renders the result range and, when there is more than one page, first, previous, numbered, next, and last buttons. Hidden when <see cref="TotalCount"/> is zero.
/// </summary>
public partial class Pager
{
    private readonly string pageSizeId = "nf-pager-" + Guid.NewGuid().ToString("N");

    private static readonly int[] DefaultPageSizes = [12, 24, 48, 96];

    /// <summary>
    /// The page whose buttons and range are shown. Required. Defaults to 1. A click that lands on this page does not raise <see cref="PageNumberChanged"/>.
    /// </summary>
    [Parameter, EditorRequired]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Items per page, used for the range and the page count. Required. Defaults to 24.
    /// </summary>
    [Parameter, EditorRequired]
    public int PageSize { get; set; } = 24;

    /// <summary>
    /// Number of items. Required. Zero hides the pager. Otherwise the range reads as the first and last item on this page of this total.
    /// </summary>
    [Parameter, EditorRequired]
    public int TotalCount { get; set; }

    /// <summary>
    /// Raised with the destination page when a page button is clicked and that page is not <see cref="PageNumber"/>. The page is clamped to the available pages.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    /// <summary>
    /// Raised with the chosen size when the per-page list changes to a number. The list is hidden when this callback has no delegate. A value that is not an integer is ignored.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    /// <summary>
    /// Choices in the per-page list. <see langword="null"/> or empty uses 12, 24, 48, and 96.
    /// </summary>
    [Parameter]
    public IReadOnlyList<int>? PageSizes { get; set; }

    /// <summary>
    /// Accessible name of the page navigation. Defaults to <c>Pagination</c>.
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