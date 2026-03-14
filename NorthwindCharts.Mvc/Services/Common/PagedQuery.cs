namespace NorthwindCharts.Mvc.Services.Common;

public sealed class PagedQuery
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public string? SortBy { get; init; }

    public bool Descending { get; init; }

    public int Skip => (Math.Max(Page, 1) - 1) * Math.Clamp(PageSize, 1, 100);

    public int Take => Math.Clamp(PageSize, 1, 100);
}
