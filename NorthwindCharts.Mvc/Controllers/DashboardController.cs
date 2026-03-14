using Microsoft.AspNetCore.Mvc;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.ViewModels.Dashboard;

namespace NorthwindCharts.Mvc.Controllers;

[Route("dashboard")]
public sealed class DashboardController : Controller
{
    private readonly IDashboardReportService _reportService;

    public DashboardController(IDashboardReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("")]
    public IActionResult Index([FromQuery] int year = 1997, [FromQuery] int topN = 5)
    {
        return View(new DashboardIndexVm { Year = year, TopN = topN });
    }

    [HttpGet("api/top-products")]
    public async Task<IActionResult> TopProducts([FromQuery] int topN = 5, CancellationToken cancellationToken = default)
    {
        var data = await _reportService.GetTopProductsAsync(topN, cancellationToken);
        return Ok(data);
    }

    [HttpGet("api/monthly-sales")]
    public async Task<IActionResult> MonthlySales([FromQuery] int year = 1997, CancellationToken cancellationToken = default)
    {
        var data = await _reportService.GetMonthlySalesAsync(year, cancellationToken);
        return Ok(data);
    }

    [HttpGet("api/sales-by-category")]
    public async Task<IActionResult> SalesByCategory([FromQuery] int year = 1997, [FromQuery] int topN = 8, CancellationToken cancellationToken = default)
    {
        var data = await _reportService.GetSalesByCategoryAsync(year, topN, cancellationToken);
        return Ok(data);
    }

    [HttpGet("api/orders-by-country")]
    public async Task<IActionResult> OrdersByCountry([FromQuery] int year = 1997, [FromQuery] int topN = 10, CancellationToken cancellationToken = default)
    {
        var data = await _reportService.GetOrdersByCountryAsync(year, topN, cancellationToken);
        return Ok(data);
    }
}
