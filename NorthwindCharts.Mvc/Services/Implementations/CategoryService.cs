using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Categories;

namespace NorthwindCharts.Mvc.Services.Implementations;

public sealed class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CategoryListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var categories = _db.Categories.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            categories = categories.Where(c => c.CategoryName.Contains(search));
        }

        categories = query.Descending ? categories.OrderByDescending(c => c.CategoryName) : categories.OrderBy(c => c.CategoryName);

        var total = await categories.CountAsync(cancellationToken);
        var items = await categories
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(c => new CategoryListItemVm
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                ProductCount = c.Products.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CategoryListItemVm>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.Take
        };
    }

    public async Task<CategoryDetailsVm?> GetDetailsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => c.CategoryId == categoryId)
            .Select(c => new CategoryDetailsVm
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                ProductCount = c.Products.Count
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoryFormVm?> GetFormAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => c.CategoryId == categoryId)
            .Select(c => new CategoryFormVm
            {
                CategoryName = c.CategoryName,
                Description = c.Description
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<int>> CreateAsync(CategoryFormVm form, CancellationToken cancellationToken = default)
    {
        var entity = new Category
        {
            CategoryName = form.CategoryName.Trim(),
            Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim()
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<int>.Ok(entity.CategoryId);
    }

    public async Task<ServiceResult> UpdateAsync(int categoryId, CategoryFormVm form, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Categories.SingleOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Category not found.");
        }

        entity.CategoryName = form.CategoryName.Trim();
        entity.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<CategoryDetailsVm>> GetDeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var entity = await GetDetailsAsync(categoryId, cancellationToken);
        return entity is null ? ServiceResult<CategoryDetailsVm>.Fail("Category not found.") : ServiceResult<CategoryDetailsVm>.Ok(entity);
    }

    public async Task<ServiceResult> DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Categories
            .Include(c => c.Products)
            .SingleOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);

        if (entity is null)
        {
            return ServiceResult.Fail("Category not found.");
        }

        if (entity.Products.Count > 0)
        {
            return ServiceResult.Fail("Category cannot be deleted because products reference it.");
        }

        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }
}
