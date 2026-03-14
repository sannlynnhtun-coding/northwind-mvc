using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.ViewModels.Dashboard;

namespace NorthwindCharts.Mvc.Services.Implementations;

public sealed class DashboardReportService : IDashboardReportService
{
    private readonly AppDbContext _db;

    public DashboardReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TopProductVm>> GetTopProductsAsync(int topN, CancellationToken cancellationToken = default)
    {
        var safeTop = Math.Clamp(topN, 1, 25);
        var rows = await _db.Database
            .SqlQueryRaw<TopProductRow>("EXEC dbo.usp_DashboardTopProducts @TopN = {0}", safeTop)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new TopProductVm
        {
            ProductName = r.ProductName,
            TotalQuantity = r.TotalQuantity
        }).ToList();
    }

    public async Task<IReadOnlyList<MonthlySalesVm>> GetMonthlySalesAsync(int year, CancellationToken cancellationToken = default)
    {
        var safeYear = Math.Clamp(year, 1990, 2100);
        var rows = await _db.Database
            .SqlQueryRaw<MonthlySalesRow>("EXEC dbo.usp_DashboardMonthlySales @Year = {0}", safeYear)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new MonthlySalesVm
        {
            SalesMonth = r.SalesMonth,
            TotalSales = r.TotalSales
        }).ToList();
    }

    public async Task<IReadOnlyList<CategorySalesVm>> GetSalesByCategoryAsync(int year, int topN, CancellationToken cancellationToken = default)
    {
        var safeYear = Math.Clamp(year, 1990, 2100);
        var safeTop = Math.Clamp(topN, 1, 25);
        var rows = await _db.Database
            .SqlQueryRaw<CategorySalesRow>("EXEC dbo.usp_DashboardSalesByCategory @Year = {0}, @TopN = {1}", safeYear, safeTop)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new CategorySalesVm
        {
            CategoryName = r.CategoryName,
            TotalSales = r.TotalSales
        }).ToList();
    }

    public async Task<IReadOnlyList<CountryOrdersVm>> GetOrdersByCountryAsync(int year, int topN, CancellationToken cancellationToken = default)
    {
        var safeYear = Math.Clamp(year, 1990, 2100);
        var safeTop = Math.Clamp(topN, 1, 25);
        var rows = await _db.Database
            .SqlQueryRaw<CountryOrdersRow>("EXEC dbo.usp_DashboardOrdersByCountry @Year = {0}, @TopN = {1}", safeYear, safeTop)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new CountryOrdersVm
        {
            ShipCountry = r.ShipCountry,
            TotalOrders = r.TotalOrders
        }).ToList();
    }

    private sealed class TopProductRow
    {
        public string ProductName { get; set; } = string.Empty;

        public int TotalQuantity { get; set; }
    }

    private sealed class MonthlySalesRow
    {
        public int SalesMonth { get; set; }

        public decimal TotalSales { get; set; }
    }

    private sealed class CategorySalesRow
    {
        public string CategoryName { get; set; } = string.Empty;

        public decimal TotalSales { get; set; }
    }

    private sealed class CountryOrdersRow
    {
        public string ShipCountry { get; set; } = string.Empty;

        public int TotalOrders { get; set; }
    }
}
