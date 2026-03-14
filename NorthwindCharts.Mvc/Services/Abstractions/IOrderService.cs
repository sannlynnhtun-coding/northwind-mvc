using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Orders;

namespace NorthwindCharts.Mvc.Services.Abstractions;

public interface IOrderService
{
    Task<PagedResult<OrderListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default);

    Task<OrderEditVm?> GetDetailsAsync(int orderId, CancellationToken cancellationToken = default);

    Task<OrderCreateVm?> GetHeaderFormAsync(int orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductOptionVm>> GetProductOptionsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<int>> CreateAsync(OrderCreateVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateHeaderAsync(int orderId, OrderCreateVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult> AddLineAsync(int orderId, AddOrderLineVm line, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateLineAsync(int orderId, int productId, OrderLineInputVm line, CancellationToken cancellationToken = default);

    Task<ServiceResult> RemoveLineAsync(int orderId, int productId, CancellationToken cancellationToken = default);

    Task<ServiceResult> ShipAsync(int orderId, CancellationToken cancellationToken = default);

    Task<ServiceResult<OrderDeleteVm>> GetDeleteAsync(int orderId, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(int orderId, CancellationToken cancellationToken = default);
}
