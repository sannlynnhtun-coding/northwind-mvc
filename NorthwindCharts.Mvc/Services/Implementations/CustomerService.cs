using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Customers;
using NorthwindCharts.Mvc.ViewModels.Orders;

namespace NorthwindCharts.Mvc.Services.Implementations;

public sealed class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CustomerListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var customers = _db.Customers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            customers = customers.Where(c => c.CustomerId.Contains(search) || c.CompanyName.Contains(search));
        }

        customers = query.Descending ? customers.OrderByDescending(c => c.CompanyName) : customers.OrderBy(c => c.CompanyName);

        var total = await customers.CountAsync(cancellationToken);
        var items = await customers
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(c => new CustomerListItemVm
            {
                CustomerId = c.CustomerId,
                CompanyName = c.CompanyName,
                ContactName = c.ContactName,
                City = c.City,
                Country = c.Country,
                Phone = c.Phone,
                OrderCount = c.Orders.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerListItemVm>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.Take
        };
    }

    public async Task<CustomerDetailsVm?> GetDetailsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == customerId)
            .Select(c => new CustomerDetailsVm
            {
                CustomerId = c.CustomerId,
                CompanyName = c.CompanyName,
                ContactName = c.ContactName,
                ContactTitle = c.ContactTitle,
                Address = c.Address,
                City = c.City,
                Region = c.Region,
                PostalCode = c.PostalCode,
                Country = c.Country,
                Phone = c.Phone,
                Fax = c.Fax,
                RecentOrders = c.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new CustomerOrderSummaryVm
                    {
                        OrderId = o.OrderId,
                        OrderDate = o.OrderDate,
                        ShippedDate = o.ShippedDate,
                        Freight = o.Freight
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<CustomerFormVm?> GetFormAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == customerId)
            .Select(c => new CustomerFormVm
            {
                CustomerId = c.CustomerId,
                CompanyName = c.CompanyName,
                ContactName = c.ContactName,
                ContactTitle = c.ContactTitle,
                Address = c.Address,
                City = c.City,
                Region = c.Region,
                PostalCode = c.PostalCode,
                Country = c.Country,
                Phone = c.Phone,
                Fax = c.Fax
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerOptionVm>> GetCustomerOptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.CompanyName)
            .Select(c => new CustomerOptionVm
            {
                Id = c.CustomerId,
                Name = $"{c.CompanyName} ({c.CustomerId})"
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<string>> CreateAsync(CustomerFormVm form, CancellationToken cancellationToken = default)
    {
        var id = form.CustomerId.Trim().ToUpperInvariant();
        var exists = await _db.Customers.AnyAsync(c => c.CustomerId == id, cancellationToken);
        if (exists)
        {
            return ServiceResult<string>.Fail("Customer ID already exists.");
        }

        var entity = new Customer
        {
            CustomerId = id,
            CompanyName = form.CompanyName.Trim(),
            ContactName = TrimOrNull(form.ContactName),
            ContactTitle = TrimOrNull(form.ContactTitle),
            Address = TrimOrNull(form.Address),
            City = TrimOrNull(form.City),
            Region = TrimOrNull(form.Region),
            PostalCode = TrimOrNull(form.PostalCode),
            Country = TrimOrNull(form.Country),
            Phone = TrimOrNull(form.Phone),
            Fax = TrimOrNull(form.Fax)
        };

        _db.Customers.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<string>.Ok(entity.CustomerId);
    }

    public async Task<ServiceResult> UpdateAsync(string customerId, CustomerFormVm form, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Customers.SingleOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Customer not found.");
        }

        entity.CompanyName = form.CompanyName.Trim();
        entity.ContactName = TrimOrNull(form.ContactName);
        entity.ContactTitle = TrimOrNull(form.ContactTitle);
        entity.Address = TrimOrNull(form.Address);
        entity.City = TrimOrNull(form.City);
        entity.Region = TrimOrNull(form.Region);
        entity.PostalCode = TrimOrNull(form.PostalCode);
        entity.Country = TrimOrNull(form.Country);
        entity.Phone = TrimOrNull(form.Phone);
        entity.Fax = TrimOrNull(form.Fax);

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<CustomerDetailsVm>> GetDeleteAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var entity = await GetDetailsAsync(customerId, cancellationToken);
        return entity is null ? ServiceResult<CustomerDetailsVm>.Fail("Customer not found.") : ServiceResult<CustomerDetailsVm>.Ok(entity);
    }

    public async Task<ServiceResult> DeleteAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Customers.Include(c => c.Orders).SingleOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Customer not found.");
        }

        if (entity.Orders.Count > 0)
        {
            return ServiceResult.Fail("Customer cannot be deleted because orders reference it.");
        }

        _db.Customers.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
