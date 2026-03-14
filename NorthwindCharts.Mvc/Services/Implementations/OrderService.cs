using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Orders;

namespace NorthwindCharts.Mvc.Services.Implementations;

public sealed class OrderService : IOrderService
{
    private const float MaxDiscount = 0.5f;

    private readonly AppDbContext _db;
    private readonly IOrderCalculator _calculator;

    public OrderService(AppDbContext db, IOrderCalculator calculator)
    {
        _db = db;
        _calculator = calculator;
    }

    public async Task<PagedResult<OrderListItemVm>> SearchAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var orders = _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            orders = orders.Where(o =>
                (o.CustomerId != null && o.CustomerId.Contains(search)) ||
                (o.Customer != null && o.Customer.CompanyName.Contains(search)) ||
                o.OrderId.ToString().Contains(search));
        }

        orders = query.Descending ? orders.OrderByDescending(o => o.OrderDate) : orders.OrderBy(o => o.OrderDate);

        var total = await orders.CountAsync(cancellationToken);
        var items = await orders
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(o => new OrderListItemVm
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.CompanyName : null,
                OrderDate = o.OrderDate,
                ShippedDate = o.ShippedDate,
                Freight = o.Freight
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderListItemVm>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.Take
        };
    }

    public async Task<OrderEditVm?> GetDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order is null)
        {
            return null;
        }

        var lines = order.OrderDetails
            .OrderBy(od => od.ProductId)
            .Select(od => new OrderLineVm
            {
                ProductId = od.ProductId,
                ProductName = od.Product?.ProductName ?? $"Product {od.ProductId}",
                UnitPrice = od.UnitPrice,
                Quantity = od.Quantity,
                Discount = od.Discount,
                LineSubtotal = _calculator.CalculateLineSubtotal(od)
            })
            .ToList();

        var totals = _calculator.CalculateTotals(order.OrderDetails, order.Freight ?? 0m);

        return new OrderEditVm
        {
            OrderId = order.OrderId,
            Header = MapHeader(order),
            Lines = lines,
            Subtotal = totals.Subtotal,
            GrandTotal = totals.GrandTotal,
            IsShipped = order.ShippedDate.HasValue
        };
    }

    public async Task<OrderCreateVm?> GetHeaderFormAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        return order is null ? null : MapHeader(order);
    }

    public async Task<IReadOnlyList<ProductOptionVm>> GetProductOptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => !p.Discontinued)
            .OrderBy(p => p.ProductName)
            .Select(p => new ProductOptionVm
            {
                Id = p.ProductId,
                Name = p.ProductName,
                UnitPrice = p.UnitPrice ?? 0m,
                UnitsInStock = p.UnitsInStock
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<int>> CreateAsync(OrderCreateVm form, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateHeaderAsync(form, cancellationToken);
        if (!validation.Success)
        {
            return ServiceResult<int>.Fail(validation.Error!);
        }

        var entity = new Order
        {
            CustomerId = form.CustomerId.Trim().ToUpperInvariant(),
            EmployeeId = form.EmployeeId,
            OrderDate = DateTime.UtcNow,
            RequiredDate = form.RequiredDate,
            ShipVia = form.ShipVia,
            Freight = form.Freight ?? 0m,
            ShipName = TrimOrNull(form.ShipName),
            ShipAddress = TrimOrNull(form.ShipAddress),
            ShipCity = TrimOrNull(form.ShipCity),
            ShipRegion = TrimOrNull(form.ShipRegion),
            ShipPostalCode = TrimOrNull(form.ShipPostalCode),
            ShipCountry = TrimOrNull(form.ShipCountry)
        };

        _db.Orders.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<int>.Ok(entity.OrderId);
    }

    public async Task<ServiceResult> UpdateHeaderAsync(int orderId, OrderCreateVm form, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Orders.SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail("Order not found.");
        }

        if (entity.ShippedDate.HasValue)
        {
            return ServiceResult.Fail("Shipped orders cannot be modified.");
        }

        var validation = await ValidateHeaderAsync(form, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        entity.CustomerId = form.CustomerId.Trim().ToUpperInvariant();
        entity.EmployeeId = form.EmployeeId;
        entity.RequiredDate = form.RequiredDate;
        entity.ShipVia = form.ShipVia;
        entity.Freight = form.Freight ?? 0m;
        entity.ShipName = TrimOrNull(form.ShipName);
        entity.ShipAddress = TrimOrNull(form.ShipAddress);
        entity.ShipCity = TrimOrNull(form.ShipCity);
        entity.ShipRegion = TrimOrNull(form.ShipRegion);
        entity.ShipPostalCode = TrimOrNull(form.ShipPostalCode);
        entity.ShipCountry = TrimOrNull(form.ShipCountry);

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AddLineAsync(int orderId, AddOrderLineVm line, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order is null)
        {
            return ServiceResult.Fail("Order not found.");
        }

        if (order.ShippedDate.HasValue)
        {
            return ServiceResult.Fail("Cannot modify lines for a shipped order.");
        }

        var validation = ValidateLine(line.Quantity, line.Discount);
        if (!validation.Success)
        {
            return validation;
        }

        var existing = order.OrderDetails.SingleOrDefault(od => od.ProductId == line.ProductId);
        if (existing is not null)
        {
            return ServiceResult.Fail("Line already exists. Use update line action.");
        }

        var product = await _db.Products.SingleOrDefaultAsync(p => p.ProductId == line.ProductId, cancellationToken);
        if (product is null)
        {
            return ServiceResult.Fail("Product not found.");
        }

        if (product.Discontinued)
        {
            return ServiceResult.Fail("Cannot add discontinued product to order.");
        }

        if ((product.UnitsInStock ?? 0) < line.Quantity)
        {
            return ServiceResult.Fail("Insufficient stock for selected product.");
        }

        order.OrderDetails.Add(new OrderDetail
        {
            OrderId = orderId,
            ProductId = product.ProductId,
            UnitPrice = product.UnitPrice ?? 0m,
            Quantity = line.Quantity,
            Discount = line.Discount
        });

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateLineAsync(int orderId, int productId, OrderLineInputVm line, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order is null)
        {
            return ServiceResult.Fail("Order not found.");
        }

        if (order.ShippedDate.HasValue)
        {
            return ServiceResult.Fail("Cannot modify lines for a shipped order.");
        }

        var existing = order.OrderDetails.SingleOrDefault(od => od.ProductId == productId);
        if (existing is null)
        {
            return ServiceResult.Fail("Order line not found.");
        }

        var validation = ValidateLine(line.Quantity, line.Discount);
        if (!validation.Success)
        {
            return validation;
        }

        var product = await _db.Products.SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (product is null)
        {
            return ServiceResult.Fail("Product not found.");
        }

        if ((product.UnitsInStock ?? 0) < line.Quantity)
        {
            return ServiceResult.Fail("Insufficient stock for selected product.");
        }

        existing.Quantity = line.Quantity;
        existing.Discount = line.Discount;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RemoveLineAsync(int orderId, int productId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order is null)
        {
            return ServiceResult.Fail("Order not found.");
        }

        if (order.ShippedDate.HasValue)
        {
            return ServiceResult.Fail("Cannot modify lines for a shipped order.");
        }

        var existing = order.OrderDetails.SingleOrDefault(od => od.ProductId == productId);
        if (existing is null)
        {
            return ServiceResult.Fail("Order line not found.");
        }

        _db.OrderDetails.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ShipAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order is null)
        {
            return ServiceResult.Fail("Order not found.");
        }

        if (order.ShippedDate.HasValue)
        {
            return ServiceResult.Fail("Order is already shipped.");
        }

        if (order.OrderDetails.Count == 0)
        {
            return ServiceResult.Fail("Order must have at least one line before shipping.");
        }

        var productIds = order.OrderDetails.Select(od => od.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);

        foreach (var line in order.OrderDetails)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
            {
                return ServiceResult.Fail($"Product {line.ProductId} not found.");
            }

            if ((product.UnitsInStock ?? 0) < line.Quantity)
            {
                return ServiceResult.Fail($"Insufficient stock for product {product.ProductName}.");
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        foreach (var line in order.OrderDetails)
        {
            var product = products[line.ProductId];
            product.UnitsInStock = (short)((product.UnitsInStock ?? 0) - line.Quantity);
        }

        order.ShippedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<OrderDeleteVm>> GetDeleteAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var data = await _db.Orders
            .AsNoTracking()
            .Where(o => o.OrderId == orderId)
            .Select(o => new OrderDeleteVm
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                IsShipped = o.ShippedDate.HasValue
            })
            .SingleOrDefaultAsync(cancellationToken);

        return data is null ? ServiceResult<OrderDeleteVm>.Fail("Order not found.") : ServiceResult<OrderDeleteVm>.Ok(data);
    }

    public async Task<ServiceResult> DeleteAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order is null)
        {
            return ServiceResult.Fail("Order not found.");
        }

        if (order.ShippedDate.HasValue)
        {
            return ServiceResult.Fail("Shipped orders cannot be deleted.");
        }

        _db.OrderDetails.RemoveRange(order.OrderDetails);
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    private async Task<ServiceResult> ValidateHeaderAsync(OrderCreateVm form, CancellationToken cancellationToken)
    {
        var customerId = form.CustomerId.Trim().ToUpperInvariant();
        var customerExists = await _db.Customers.AnyAsync(c => c.CustomerId == customerId, cancellationToken);
        if (!customerExists)
        {
            return ServiceResult.Fail("Customer does not exist.");
        }

        if (form.ShipVia.HasValue)
        {
            var shipperExists = await _db.Shippers.AnyAsync(s => s.ShipperId == form.ShipVia.Value, cancellationToken);
            if (!shipperExists)
            {
                return ServiceResult.Fail("Selected shipper does not exist.");
            }
        }

        return ServiceResult.Ok();
    }

    private static ServiceResult ValidateLine(short quantity, float discount)
    {
        if (quantity <= 0)
        {
            return ServiceResult.Fail("Quantity must be greater than zero.");
        }

        if (discount < 0f || discount > MaxDiscount)
        {
            return ServiceResult.Fail($"Discount must be between 0 and {MaxDiscount:0.##}.");
        }

        return ServiceResult.Ok();
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static OrderCreateVm MapHeader(Order order)
    {
        return new OrderCreateVm
        {
            CustomerId = order.CustomerId ?? string.Empty,
            EmployeeId = order.EmployeeId,
            ShipVia = order.ShipVia,
            RequiredDate = order.RequiredDate,
            Freight = order.Freight,
            ShipName = order.ShipName,
            ShipAddress = order.ShipAddress,
            ShipCity = order.ShipCity,
            ShipRegion = order.ShipRegion,
            ShipPostalCode = order.ShipPostalCode,
            ShipCountry = order.ShipCountry
        };
    }
}


