namespace Heroicsoft.NetForge.Components.Paging;

/// <summary>Page-window maths for <c>Pager</c>, kept separate so the windowing can be tested.</summary>
public static class PagerMath
{
    /// <summary>
    /// Counts pages for a result set. A <paramref name="pageSize"/> of zero or less yields 1, and the result is never less than 1.
    /// </summary>
    /// <param name="totalCount">Number of items. Zero still yields one page.</param>
    /// <param name="pageSize">Items per page. Zero or less yields 1.</param>
    /// <returns>The page count, at least 1.</returns>
    public static int PageCount(int totalCount, int pageSize) =>
        pageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

    /// <summary>
    /// Returns the 1-based index of the first item on the page. Does not look at the total, so the index can fall past the last item.
    /// </summary>
    /// <param name="pageNumber">The page. Values below 1 are treated as 1.</param>
    /// <param name="pageSize">Items per page. Values below 1 are treated as 1.</param>
    /// <returns>The 1-based index of the first item.</returns>
    public static int FirstOnPage(int pageNumber, int pageSize) =>
        ((Math.Max(pageNumber, 1) - 1) * Math.Max(pageSize, 1)) + 1;

    /// <summary>
    /// Returns the 1-based index of the last item on the page, capped at <paramref name="totalCount"/>.
    /// </summary>
    /// <param name="pageNumber">The page. Values below 1 are treated as 1.</param>
    /// <param name="pageSize">Items per page. Values below 1 are treated as 1.</param>
    /// <param name="totalCount">Number of items. Negative values are treated as zero, so the result can be 0.</param>
    /// <returns>The 1-based index of the last item, or 0 when nothing is in range.</returns>
    public static int LastOnPage(int pageNumber, int pageSize, int totalCount) =>
        Math.Min(Math.Max(pageNumber, 1) * Math.Max(pageSize, 1), Math.Max(totalCount, 0));

    /// <summary>A window around the current page, so hundreds of pages don't produce hundreds of buttons.</summary>
    /// <param name="pageNumber">The current page, clamped into the available pages.</param>
    /// <param name="pageCount">How many pages exist. Less than 1 returns an empty list.</param>
    /// <param name="window">How many page numbers to return. Defaults to 5. Values below 1 are treated as 1.</param>
    /// <returns>The page numbers to show, centered on the current page when the ends allow it.</returns>
    public static IReadOnlyList<int> VisiblePages(int pageNumber, int pageCount, int window = 5)
    {
        if (pageCount < 1)
        {
            return [];
        }

        int size = Math.Max(1, window);
        int current = Math.Clamp(pageNumber, 1, pageCount);
        int first = Math.Max(1, current - (size / 2));
        int last = Math.Min(pageCount, first + size - 1);
        first = Math.Max(1, last - size + 1);

        return Enumerable.Range(first, last - first + 1).ToArray();
    }
}