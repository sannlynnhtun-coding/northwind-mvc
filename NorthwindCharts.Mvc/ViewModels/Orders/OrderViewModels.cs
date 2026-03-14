using NorthwindCharts.Mvc.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace NorthwindCharts.Mvc.ViewModels.Orders;

public sealed class OrderListItemVm
{
    public int OrderId { get; init; }

    public string? CustomerId { get; init; }

    public string? CustomerName { get; init; }

    public DateTime? OrderDate { get; init; }

    public DateTime? ShippedDate { get; init; }

    public decimal? Freight { get; init; }

    public bool IsShipped => ShippedDate.HasValue;
}

public sealed class OrderIndexVm
{
    public required IReadOnlyList<OrderListItemVm> Items { get; init; }

    public required PaginationVm Pagination { get; init; }
}

public sealed class OrderCreateVm
{
    [Required]
    [StringLength(5, MinimumLength = 5)]
    public string CustomerId { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }

    public int? ShipVia { get; set; }

    public DateTime? RequiredDate { get; set; }

    [Range(0, 100000)]
    public decimal? Freight { get; set; }

    [StringLength(40)]
    public string? ShipName { get; set; }

    [StringLength(60)]
    public string? ShipAddress { get; set; }

    [StringLength(15)]
    public string? ShipCity { get; set; }

    [StringLength(15)]
    public string? ShipRegion { get; set; }

    [StringLength(10)]
    public string? ShipPostalCode { get; set; }

    [StringLength(15)]
    public string? ShipCountry { get; set; }
}

public sealed class OrderLineVm
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public short Quantity { get; init; }

    public float Discount { get; init; }

    public decimal LineSubtotal { get; init; }
}

public sealed class OrderEditVm
{
    public int OrderId { get; init; }

    public required OrderCreateVm Header { get; init; }

    public required IReadOnlyList<OrderLineVm> Lines { get; init; }

    public decimal Subtotal { get; init; }

    public decimal GrandTotal { get; init; }

    public bool IsShipped { get; init; }
}

public class OrderLineInputVm
{
    [Range(1, short.MaxValue)]
    public short Quantity { get; set; }

    [Range(0, 0.5)]
    public float Discount { get; set; }
}

public sealed class AddOrderLineVm : OrderLineInputVm
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
}

public sealed class OrderDeleteVm
{
    public int OrderId { get; init; }

    public string? CustomerId { get; init; }

    public DateTime? OrderDate { get; init; }

    public bool IsShipped { get; init; }
}

public sealed class OrderFormPageVm
{
    public required OrderCreateVm Form { get; init; }

    public required IReadOnlyList<CustomerOptionVm> Customers { get; init; }

    public required IReadOnlyList<ShipperOptionVm> Shippers { get; init; }
}

public sealed class OrderEditPageVm
{
    public required OrderEditVm Order { get; init; }

    public required IReadOnlyList<ProductOptionVm> ProductOptions { get; init; }

    public required IReadOnlyList<CustomerOptionVm> Customers { get; init; }

    public required IReadOnlyList<ShipperOptionVm> Shippers { get; init; }

    public AddOrderLineVm AddLine { get; init; } = new();
}

public sealed class CustomerOptionVm
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}

public sealed class ShipperOptionVm
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class ProductOptionVm
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public short? UnitsInStock { get; init; }
}

