using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Products;

namespace NorthwindCharts.Mvc.Services.Abstractions;

public interface IProductService
{
    Task<PagedResult<ProductListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default);

    Task<ProductDetailsVm?> GetDetailsAsync(int productId, CancellationToken cancellationToken = default);

    Task<ProductFormVm?> GetFormAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupVm>> GetCategoryOptionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupVm>> GetSupplierOptionsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<int>> CreateAsync(ProductFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(int productId, ProductFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult<ProductDeleteVm>> GetDeleteAsync(int productId, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(int productId, CancellationToken cancellationToken = default);
}
