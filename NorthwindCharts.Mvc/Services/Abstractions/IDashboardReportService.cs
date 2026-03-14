using NorthwindCharts.Mvc.ViewModels.Dashboard;

namespace NorthwindCharts.Mvc.Services.Abstractions;

public interface IDashboardReportService
{
    Task<IReadOnlyList<TopProductVm>> GetTopProductsAsync(int topN, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlySalesVm>> GetMonthlySalesAsync(int year, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategorySalesVm>> GetSalesByCategoryAsync(int year, int topN, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CountryOrdersVm>> GetOrdersByCountryAsync(int year, int topN, CancellationToken cancellationToken = default);
}
