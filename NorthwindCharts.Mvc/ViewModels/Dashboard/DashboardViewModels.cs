namespace NorthwindCharts.Mvc.ViewModels.Dashboard;

public sealed class TopProductVm
{
    public string ProductName { get; init; } = string.Empty;

    public int TotalQuantity { get; init; }
}

public sealed class MonthlySalesVm
{
    public int SalesMonth { get; init; }

    public decimal TotalSales { get; init; }
}

public sealed class CategorySalesVm
{
    public string CategoryName { get; init; } = string.Empty;

    public decimal TotalSales { get; init; }
}

public sealed class CountryOrdersVm
{
    public string ShipCountry { get; init; } = string.Empty;

    public int TotalOrders { get; init; }
}

public sealed class DashboardIndexVm
{
    public int Year { get; init; } = 1997;

    public int TopN { get; init; } = 5;
}
