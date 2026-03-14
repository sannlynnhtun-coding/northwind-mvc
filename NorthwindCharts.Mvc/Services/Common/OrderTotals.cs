namespace NorthwindCharts.Mvc.Services.Common;

public sealed class OrderTotals
{
    public decimal Subtotal { get; init; }

    public decimal Freight { get; init; }

    public decimal GrandTotal => Subtotal + Freight;
}
