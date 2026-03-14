using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Products;
using NorthwindCharts.Mvc.ViewModels.Suppliers;

namespace NorthwindCharts.Mvc.Services.Implementations;

public sealed class SupplierService : ISupplierService
{
    private readonly AppDbContext _db;

    public SupplierService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<SupplierListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var suppliers = _db.Suppliers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            suppliers = suppliers.Where(s => s.CompanyName.Contains(search));
        }

        suppliers = query.Descending ? suppliers.OrderByDescending(s => s.CompanyName) : suppliers.OrderBy(s => s.CompanyName);

        var total = await suppliers.CountAsync(cancellationToken);
        var items = await suppliers
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(s => new SupplierListItemVm
            {
                SupplierId = s.SupplierId,
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                City = s.City,
                Country = s.Country,
                Phone = s.Phone,
                ProductCount = s.Products.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<SupplierListItemVm>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.Take
        };
    }

    public async Task<SupplierDetailsVm?> GetDetailsAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        return await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.SupplierId == supplierId)
            .Select(s => new SupplierDetailsVm
            {
                SupplierId = s.SupplierId,
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                ContactTitle = s.ContactTitle,
                Address = s.Address,
                City = s.City,
                Region = s.Region,
                PostalCode = s.PostalCode,
                Country = s.Country,
                Phone = s.Phone,
                Fax = s.Fax,
                ProductCount = s.Products.Count
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SupplierFormVm?> GetFormAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        return await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.SupplierId == supplierId)
            .Select(s => new SupplierFormVm
            {
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                ContactTitle = s.ContactTitle,
                Address = s.Address,
                City = s.City,
                Region = s.Region,
                PostalCode = s.PostalCode,
                Country = s.Country,
                Phone = s.Phone,
                Fax = s.Fax,
                HomePage = s.HomePage
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupVm>> GetSupplierOptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.CompanyName)
            .Select(s => new LookupVm { Id = s.SupplierId, Name = s.CompanyName })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<int>> CreateAsync(SupplierFormVm form, CancellationToken cancellationToken = default)
    {
        var entity = new Supplier
        {
            CompanyName = form.CompanyName.Trim(),
            ContactName = TrimOrNull(form.ContactName),
            ContactTitle = TrimOrNull(form.ContactTitle),
            Address = TrimOrNull(form.Address),
            City = TrimOrNull(form.City),
            Region = TrimOrNull(form.Region),
            PostalCode = TrimOrNull(form.PostalCode),
            Country = TrimOrNull(form.Country),
            Phone = TrimOrNull(form.Phone),
            Fax = TrimOrNull(form.Fax),
            HomePage = TrimOrNull(form.HomePage)
        };

        _db.Suppliers.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<int>.Ok(entity.SupplierId);
    }

    public async Task<ServiceResult> UpdateAsync(int supplierId, SupplierFormVm form, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Suppliers.SingleOrDefaultAsync(s => s.SupplierId == supplierId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Supplier not found.");
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
        entity.HomePage = TrimOrNull(form.HomePage);

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<SupplierDetailsVm>> GetDeleteAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var entity = await GetDetailsAsync(supplierId, cancellationToken);
        return entity is null ? ServiceResult<SupplierDetailsVm>.Fail("Supplier not found.") : ServiceResult<SupplierDetailsVm>.Ok(entity);
    }

    public async Task<ServiceResult> DeleteAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Suppliers.Include(s => s.Products).SingleOrDefaultAsync(s => s.SupplierId == supplierId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Supplier not found.");
        }

        if (entity.Products.Count > 0)
        {
            return ServiceResult.Fail("Supplier cannot be deleted because products reference it.");
        }

        _db.Suppliers.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
