namespace NorthwindCharts.Mvc.ViewModels.Shared;

public sealed class PaginationVm
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public int TotalCount { get; init; }

    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public string? Search { get; init; }
}
