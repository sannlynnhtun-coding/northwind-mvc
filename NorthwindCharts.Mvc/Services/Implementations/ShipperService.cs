using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Orders;
using NorthwindCharts.Mvc.ViewModels.Shippers;

namespace NorthwindCharts.Mvc.Services.Implementations;

public sealed class ShipperService : IShipperService
{
    private readonly AppDbContext _db;

    public ShipperService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ShipperListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var shippers = _db.Shippers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            shippers = shippers.Where(s => s.CompanyName.Contains(search));
        }

        shippers = query.Descending ? shippers.OrderByDescending(s => s.CompanyName) : shippers.OrderBy(s => s.CompanyName);

        var total = await shippers.CountAsync(cancellationToken);
        var items = await shippers
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(s => new ShipperListItemVm
            {
                ShipperId = s.ShipperId,
                CompanyName = s.CompanyName,
                Phone = s.Phone,
                OrderCount = s.Orders.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ShipperListItemVm>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.Take
        };
    }

    public async Task<ShipperDetailsVm?> GetDetailsAsync(int shipperId, CancellationToken cancellationToken = default)
    {
        return await _db.Shippers
            .AsNoTracking()
            .Where(s => s.ShipperId == shipperId)
            .Select(s => new ShipperDetailsVm
            {
                ShipperId = s.ShipperId,
                CompanyName = s.CompanyName,
                Phone = s.Phone,
                OrderCount = s.Orders.Count
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ShipperFormVm?> GetFormAsync(int shipperId, CancellationToken cancellationToken = default)
    {
        return await _db.Shippers
            .AsNoTracking()
            .Where(s => s.ShipperId == shipperId)
            .Select(s => new ShipperFormVm
            {
                CompanyName = s.CompanyName,
                Phone = s.Phone
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ShipperOptionVm>> GetShipperOptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Shippers
            .AsNoTracking()
            .OrderBy(s => s.CompanyName)
            .Select(s => new ShipperOptionVm { Id = s.ShipperId, Name = s.CompanyName })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<int>> CreateAsync(ShipperFormVm form, CancellationToken cancellationToken = default)
    {
        var entity = new Shipper
        {
            CompanyName = form.CompanyName.Trim(),
            Phone = string.IsNullOrWhiteSpace(form.Phone) ? null : form.Phone.Trim()
        };

        _db.Shippers.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<int>.Ok(entity.ShipperId);
    }

    public async Task<ServiceResult> UpdateAsync(int shipperId, ShipperFormVm form, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Shippers.SingleOrDefaultAsync(s => s.ShipperId == shipperId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Shipper not found.");
        }

        entity.CompanyName = form.CompanyName.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(form.Phone) ? null : form.Phone.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<ShipperDetailsVm>> GetDeleteAsync(int shipperId, CancellationToken cancellationToken = default)
    {
        var entity = await GetDetailsAsync(shipperId, cancellationToken);
        return entity is null ? ServiceResult<ShipperDetailsVm>.Fail("Shipper not found.") : ServiceResult<ShipperDetailsVm>.Ok(entity);
    }

    public async Task<ServiceResult> DeleteAsync(int shipperId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Shippers.Include(s => s.Orders).SingleOrDefaultAsync(s => s.ShipperId == shipperId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Shipper not found.");
        }

        if (entity.Orders.Count > 0)
        {
            return ServiceResult.Fail("Shipper cannot be deleted because orders reference it.");
        }

        _db.Shippers.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }
}
