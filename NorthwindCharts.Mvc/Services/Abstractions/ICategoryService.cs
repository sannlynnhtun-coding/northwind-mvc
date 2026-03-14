using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Categories;

namespace NorthwindCharts.Mvc.Services.Abstractions;

public interface ICategoryService
{
    Task<PagedResult<CategoryListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default);

    Task<CategoryDetailsVm?> GetDetailsAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<CategoryFormVm?> GetFormAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<ServiceResult<int>> CreateAsync(CategoryFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(int categoryId, CategoryFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult<CategoryDetailsVm>> GetDeleteAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(int categoryId, CancellationToken cancellationToken = default);
}
