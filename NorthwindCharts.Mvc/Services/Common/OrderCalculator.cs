using NorthwindCharts.Database.AppDbContextModels;

namespace NorthwindCharts.Mvc.Services.Common;

public sealed class OrderCalculator : IOrderCalculator
{
    public decimal CalculateLineSubtotal(OrderDetail detail)
    {
        var discount = Math.Clamp((decimal)detail.Discount, 0m, 1m);
        return detail.UnitPrice * detail.Quantity * (1m - discount);
    }

    public OrderTotals CalculateTotals(IEnumerable<OrderDetail> details, decimal freight)
    {
        var subtotal = details.Sum(CalculateLineSubtotal);
        return new OrderTotals
        {
            Subtotal = Math.Round(subtotal, 2),
            Freight = Math.Round(freight, 2)
        };
    }
}
