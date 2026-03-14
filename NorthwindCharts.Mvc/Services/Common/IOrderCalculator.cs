using NorthwindCharts.Database.AppDbContextModels;

namespace NorthwindCharts.Mvc.Services.Common;

public interface IOrderCalculator
{
    decimal CalculateLineSubtotal(OrderDetail detail);

    OrderTotals CalculateTotals(IEnumerable<OrderDetail> details, decimal freight);
}
