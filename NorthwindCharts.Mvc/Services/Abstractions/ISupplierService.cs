using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Products;
using NorthwindCharts.Mvc.ViewModels.Suppliers;

namespace NorthwindCharts.Mvc.Services.Abstractions;

public interface ISupplierService
{
    Task<PagedResult<SupplierListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default);

    Task<SupplierDetailsVm?> GetDetailsAsync(int supplierId, CancellationToken cancellationToken = default);

    Task<SupplierFormVm?> GetFormAsync(int supplierId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupVm>> GetSupplierOptionsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<int>> CreateAsync(SupplierFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(int supplierId, SupplierFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult<SupplierDetailsVm>> GetDeleteAsync(int supplierId, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(int supplierId, CancellationToken cancellationToken = default);
}
