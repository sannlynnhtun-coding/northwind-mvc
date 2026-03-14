using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Customers;
using NorthwindCharts.Mvc.ViewModels.Orders;

namespace NorthwindCharts.Mvc.Services.Abstractions;

public interface ICustomerService
{
    Task<PagedResult<CustomerListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default);

    Task<CustomerDetailsVm?> GetDetailsAsync(string customerId, CancellationToken cancellationToken = default);

    Task<CustomerFormVm?> GetFormAsync(string customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerOptionVm>> GetCustomerOptionsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<string>> CreateAsync(CustomerFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(string customerId, CustomerFormVm form, CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerDetailsVm>> GetDeleteAsync(string customerId, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(string customerId, CancellationToken cancellationToken = default);
}
