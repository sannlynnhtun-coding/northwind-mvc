using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Products;

namespace NorthwindCharts.Mvc.Services.Implementations;

public sealed class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProductListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var products = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            products = products.Where(p => p.ProductName.Contains(search));
        }

        products = query.SortBy?.ToLowerInvariant() switch
        {
            "price" => query.Descending ? products.OrderByDescending(p => p.UnitPrice) : products.OrderBy(p => p.UnitPrice),
            "stock" => query.Descending ? products.OrderByDescending(p => p.UnitsInStock) : products.OrderBy(p => p.UnitsInStock),
            _ => query.Descending ? products.OrderByDescending(p => p.ProductName) : products.OrderBy(p => p.ProductName)
        };

        var total = await products.CountAsync(cancellationToken);

        var items = await products
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(p => new ProductListItemVm
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                SupplierName = p.Supplier != null ? p.Supplier.CompanyName : null,
                UnitPrice = p.UnitPrice,
                UnitsInStock = p.UnitsInStock,
                Discontinued = p.Discontinued
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemVm>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.Take
        };
    }

    public async Task<ProductDetailsVm?> GetDetailsAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.ProductId == productId)
            .Select(p => new ProductDetailsVm
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                SupplierName = p.Supplier != null ? p.Supplier.CompanyName : null,
                QuantityPerUnit = p.QuantityPerUnit,
                UnitPrice = p.UnitPrice,
                UnitsInStock = p.UnitsInStock,
                UnitsOnOrder = p.UnitsOnOrder,
                ReorderLevel = p.ReorderLevel,
                Discontinued = p.Discontinued
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ProductFormVm?> GetFormAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => new ProductFormVm
            {
                ProductName = p.ProductName,
                SupplierId = p.SupplierId,
                CategoryId = p.CategoryId,
                QuantityPerUnit = p.QuantityPerUnit,
                UnitPrice = p.UnitPrice,
                UnitsInStock = p.UnitsInStock,
                UnitsOnOrder = p.UnitsOnOrder,
                ReorderLevel = p.ReorderLevel,
                Discontinued = p.Discontinued
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupVm>> GetCategoryOptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new LookupVm { Id = c.CategoryId, Name = c.CategoryName })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupVm>> GetSupplierOptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Suppliers
            .AsNoTracking()
            .OrderBy(c => c.CompanyName)
            .Select(c => new LookupVm { Id = c.SupplierId, Name = c.CompanyName })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<int>> CreateAsync(ProductFormVm form, CancellationToken cancellationToken = default)
    {
        var validation = ValidateStockRules(form);
        if (!validation.Success)
        {
            return ServiceResult<int>.Fail(validation.Error!);
        }

        var entity = new Product
        {
            ProductName = form.ProductName.Trim(),
            SupplierId = form.SupplierId,
            CategoryId = form.CategoryId,
            QuantityPerUnit = string.IsNullOrWhiteSpace(form.QuantityPerUnit) ? null : form.QuantityPerUnit.Trim(),
            UnitPrice = form.UnitPrice,
            UnitsInStock = form.UnitsInStock,
            UnitsOnOrder = form.UnitsOnOrder,
            ReorderLevel = form.ReorderLevel,
            Discontinued = form.Discontinued
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<int>.Ok(entity.ProductId);
    }

    public async Task<ServiceResult> UpdateAsync(int productId, ProductFormVm form, CancellationToken cancellationToken = default)
    {
        var validation = ValidateStockRules(form);
        if (!validation.Success)
        {
            return validation;
        }

        var entity = await _db.Products.SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Product not found.");
        }

        entity.ProductName = form.ProductName.Trim();
        entity.SupplierId = form.SupplierId;
        entity.CategoryId = form.CategoryId;
        entity.QuantityPerUnit = string.IsNullOrWhiteSpace(form.QuantityPerUnit) ? null : form.QuantityPerUnit.Trim();
        entity.UnitPrice = form.UnitPrice;
        entity.UnitsInStock = form.UnitsInStock;
        entity.UnitsOnOrder = form.UnitsOnOrder;
        entity.ReorderLevel = form.ReorderLevel;
        entity.Discontinued = form.Discontinued;

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<ProductDeleteVm>> GetDeleteAsync(int productId, CancellationToken cancellationToken = default)
    {
        var data = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => new ProductDeleteVm
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Discontinued = p.Discontinued
            })
            .SingleOrDefaultAsync(cancellationToken);

        return data is null ? ServiceResult<ProductDeleteVm>.Fail("Product not found.") : ServiceResult<ProductDeleteVm>.Ok(data);
    }

    public async Task<ServiceResult> DeleteAsync(int productId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Products.SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Product not found.");
        }

        entity.Discontinued = true;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    private static ServiceResult ValidateStockRules(ProductFormVm form)
    {
        var stock = form.UnitsInStock ?? 0;
        var reorder = form.ReorderLevel ?? 0;
        var onOrder = form.UnitsOnOrder ?? 0;

        if (!form.Discontinued && reorder > stock + onOrder)
        {
            return ServiceResult.Fail("Reorder level cannot exceed units in stock plus units on order for active products.");
        }

        return ServiceResult.Ok();
    }
}
