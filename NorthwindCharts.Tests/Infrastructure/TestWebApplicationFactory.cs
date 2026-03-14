using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.ViewModels.Dashboard;

namespace NorthwindCharts.Tests.Infrastructure;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"NorthwindChartsTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            var dbContextOptionsConfigDescriptor = services.FirstOrDefault(d =>
                d.ServiceType.IsGenericType &&
                d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal) &&
                d.ServiceType.GenericTypeArguments.Length == 1 &&
                d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext));

            if (dbContextOptionsConfigDescriptor is not null)
            {
                services.Remove(dbContextOptionsConfigDescriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.PostConfigure<MvcOptions>(options =>
            {
                options.Filters.Add(new IgnoreAntiforgeryTokenAttribute { Order = 2000 });
            });

            services.RemoveAll<IDashboardReportService>();
            services.AddScoped<IDashboardReportService, FakeDashboardReportService>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            Seed(db);
        });
    }

    private static void Seed(AppDbContext db)
    {
        if (db.Categories.Any())
        {
            return;
        }

        var category1 = new Category { CategoryId = 1, CategoryName = "Beverages" };
        var category2 = new Category { CategoryId = 2, CategoryName = "Condiments" };

        var supplier1 = new Supplier { SupplierId = 1, CompanyName = "Exotic Liquids" };
        var shipper1 = new Shipper { ShipperId = 1, CompanyName = "Speedy Express" };
        var customer1 = new Customer { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" };

        var product1 = new Product
        {
            ProductId = 1,
            ProductName = "Chai",
            CategoryId = 1,
            SupplierId = 1,
            UnitPrice = 18m,
            UnitsInStock = 40,
            Discontinued = false
        };

        db.Categories.AddRange(category1, category2);
        db.Suppliers.Add(supplier1);
        db.Shippers.Add(shipper1);
        db.Customers.Add(customer1);
        db.Products.Add(product1);

        var order = new Order
        {
            OrderId = 1,
            CustomerId = customer1.CustomerId,
            OrderDate = new DateTime(1997, 1, 10),
            ShipVia = shipper1.ShipperId,
            Freight = 10m
        };

        var detail = new OrderDetail
        {
            OrderId = 1,
            ProductId = 1,
            UnitPrice = 18m,
            Quantity = 2,
            Discount = 0
        };

        db.Orders.Add(order);
        db.OrderDetails.Add(detail);
        db.SaveChanges();
    }

    private sealed class FakeDashboardReportService : IDashboardReportService
    {
        public Task<IReadOnlyList<TopProductVm>> GetTopProductsAsync(int topN, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TopProductVm>>([
                new TopProductVm { ProductName = "Chai", TotalQuantity = 100 }
            ]);

        public Task<IReadOnlyList<MonthlySalesVm>> GetMonthlySalesAsync(int year, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MonthlySalesVm>>([
                new MonthlySalesVm { SalesMonth = 1, TotalSales = 1234.56m }
            ]);

        public Task<IReadOnlyList<CategorySalesVm>> GetSalesByCategoryAsync(int year, int topN, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CategorySalesVm>>([
                new CategorySalesVm { CategoryName = "Beverages", TotalSales = 999.50m }
            ]);

        public Task<IReadOnlyList<CountryOrdersVm>> GetOrdersByCountryAsync(int year, int topN, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CountryOrdersVm>>([
                new CountryOrdersVm { ShipCountry = "Germany", TotalOrders = 4 }
            ]);
    }
}
