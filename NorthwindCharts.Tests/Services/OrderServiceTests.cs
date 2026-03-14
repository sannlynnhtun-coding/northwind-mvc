using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.Services.Implementations;
using NorthwindCharts.Mvc.ViewModels.Orders;

namespace NorthwindCharts.Tests.Services;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task ShipAsync_DeductsStock_AndSetsShippedDate()
    {
        await using var db = BuildDb();
        SeedOrderData(db);
        var service = new OrderService(db, new OrderCalculator());

        var result = await service.ShipAsync(100);

        Assert.True(result.Success);
        var order = await db.Orders.FindAsync(100);
        var product = await db.Products.FindAsync(10);
        Assert.NotNull(order);
        Assert.NotNull(order!.ShippedDate);
        Assert.NotNull(product);
        Assert.Equal((short)15, product!.UnitsInStock);
    }

    [Fact]
    public async Task AddLineAsync_Blocks_WhenDiscountTooHigh()
    {
        await using var db = BuildDb();
        SeedOrderData(db);
        var service = new OrderService(db, new OrderCalculator());

        var result = await service.AddLineAsync(100, new AddOrderLineVm
        {
            ProductId = 10,
            Quantity = 1,
            Discount = 0.9f
        });

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    private static AppDbContext BuildDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedOrderData(AppDbContext db)
    {
        db.Customers.Add(new Customer { CustomerId = "ALFKI", CompanyName = "Alfreds" });
        db.Shippers.Add(new Shipper { ShipperId = 1, CompanyName = "Fast" });
        db.Products.Add(new Product
        {
            ProductId = 10,
            ProductName = "Tea",
            UnitPrice = 10m,
            UnitsInStock = 20,
            Discontinued = false
        });

        db.Orders.Add(new Order
        {
            OrderId = 100,
            CustomerId = "ALFKI",
            ShipVia = 1,
            OrderDate = DateTime.UtcNow,
            Freight = 5m
        });

        db.OrderDetails.Add(new OrderDetail
        {
            OrderId = 100,
            ProductId = 10,
            UnitPrice = 10m,
            Quantity = 5,
            Discount = 0f
        });

        db.SaveChanges();
    }
}
