using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Implementations;
using NorthwindCharts.Mvc.ViewModels.Categories;

namespace NorthwindCharts.Tests.Services;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task DeleteAsync_Blocks_WhenProductsExist()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var db = new AppDbContext(options);

        db.Categories.Add(new Category { CategoryId = 1, CategoryName = "Cat" });
        db.Products.Add(new Product { ProductId = 1, ProductName = "Prod", CategoryId = 1, Discontinued = false });
        db.SaveChanges();

        var service = new CategoryService(db);

        var result = await service.DeleteAsync(1);

        Assert.False(result.Success);
        Assert.Contains("cannot be deleted", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_CreatesCategory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var db = new AppDbContext(options);
        var service = new CategoryService(db);

        var result = await service.CreateAsync(new CategoryFormVm { CategoryName = "New Category" });

        Assert.True(result.Success);
        Assert.True(result.Value > 0);
    }
}
