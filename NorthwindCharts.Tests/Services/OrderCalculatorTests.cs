using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Common;

namespace NorthwindCharts.Tests.Services;

public sealed class OrderCalculatorTests
{
    [Fact]
    public void CalculateLineSubtotal_ReturnsExpectedValue()
    {
        var calculator = new OrderCalculator();
        var detail = new OrderDetail
        {
            UnitPrice = 20m,
            Quantity = 3,
            Discount = 0.1f
        };

        var subtotal = calculator.CalculateLineSubtotal(detail);

        Assert.Equal(54m, subtotal);
    }

    [Fact]
    public void CalculateTotals_ReturnsSubtotalAndGrandTotal()
    {
        var calculator = new OrderCalculator();
        var details = new List<OrderDetail>
        {
            new() { UnitPrice = 10m, Quantity = 2, Discount = 0f },
            new() { UnitPrice = 5m, Quantity = 4, Discount = 0.2f }
        };

        var totals = calculator.CalculateTotals(details, 7m);

        Assert.Equal(36m, totals.Subtotal);
        Assert.Equal(43m, totals.GrandTotal);
    }
}
