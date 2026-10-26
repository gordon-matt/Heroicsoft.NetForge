using Heroicsoft.NetForge.Components.Paging;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Paging;

public partial class Pager
{
    private readonly string pageSizeId = "bf-pager-" + Guid.NewGuid().ToString("N");

    private static readonly int[] DefaultPageSizes = [12, 24, 48, 96];

    [Parameter, EditorRequired]
    public int PageNumber { get; set; } = 1;

    [Parameter, EditorRequired]
    public int PageSize { get; set; } = 24;

    [Parameter, EditorRequired]
    public int TotalCount { get; set; }

    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    [Parameter]
    public IReadOnlyList<int>? PageSizes { get; set; }

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
