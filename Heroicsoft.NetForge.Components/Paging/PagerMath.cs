namespace Heroicsoft.NetForge.Components.Paging;

/// <summary>Page-window maths for <c>Pager</c>, kept separate so the windowing can be tested.</summary>
public static class PagerMath
{
    public static int PageCount(int totalCount, int pageSize) =>
        pageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

    public static int FirstOnPage(int pageNumber, int pageSize) =>
        ((Math.Max(pageNumber, 1) - 1) * Math.Max(pageSize, 1)) + 1;

    public static int LastOnPage(int pageNumber, int pageSize, int totalCount) =>
        Math.Min(Math.Max(pageNumber, 1) * Math.Max(pageSize, 1), Math.Max(totalCount, 0));

    /// <summary>A window around the current page, so hundreds of pages don't produce hundreds of buttons.</summary>
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