using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Orders;
using NorthwindCharts.Mvc.ViewModels.Shippers;

namespace NorthwindCharts.Mvc.Services.Abstractions;

public interface IShipperService
{
    Task<PagedResult<ShipperListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default);

    Task<ShipperDetailsVm?> GetDetailsAsync(int shipperId, CancellationToken cancellationToken = default);

    Task<ShipperFormVm?> GetFormAsync(int shipperId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShipperOptionVm>> GetShipperOptionsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<int>> CreateAsync(ShipperFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(int shipperId, ShipperFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult<ShipperDetailsVm>> GetDeleteAsync(int shipperId, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(int shipperId, CancellationToken cancellationToken = default);
}
