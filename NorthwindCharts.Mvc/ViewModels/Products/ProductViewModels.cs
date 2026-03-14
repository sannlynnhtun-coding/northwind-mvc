using NorthwindCharts.Mvc.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace NorthwindCharts.Mvc.ViewModels.Products;

public sealed class ProductListItemVm
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string? CategoryName { get; init; }

    public string? SupplierName { get; init; }

    public decimal? UnitPrice { get; init; }

    public short? UnitsInStock { get; init; }

    public bool Discontinued { get; init; }
}

public sealed class ProductIndexVm
{
    public required IReadOnlyList<ProductListItemVm> Items { get; init; }

    public required PaginationVm Pagination { get; init; }
}

public sealed class ProductFormVm
{
    [Required]
    [StringLength(40)]
    public string ProductName { get; set; } = string.Empty;

    public int? SupplierId { get; set; }

    public int? CategoryId { get; set; }

    [StringLength(20)]
    public string? QuantityPerUnit { get; set; }

    [Range(0, 100000)]
    public decimal? UnitPrice { get; set; }

    [Range(0, short.MaxValue)]
    public short? UnitsInStock { get; set; }

    [Range(0, short.MaxValue)]
    public short? UnitsOnOrder { get; set; }

    [Range(0, short.MaxValue)]
    public short? ReorderLevel { get; set; }

    public bool Discontinued { get; set; }
}

public sealed class ProductDetailsVm
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string? CategoryName { get; init; }

    public string? SupplierName { get; init; }

    public string? QuantityPerUnit { get; init; }

    public decimal? UnitPrice { get; init; }

    public short? UnitsInStock { get; init; }

    public short? UnitsOnOrder { get; init; }

    public short? ReorderLevel { get; init; }

    public bool Discontinued { get; init; }
}

public sealed class ProductDeleteVm
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public bool Discontinued { get; init; }
}

public sealed class ProductFormPageVm
{
    public int? ProductId { get; init; }

    public required ProductFormVm Form { get; init; }

    public required IReadOnlyList<LookupVm> CategoryOptions { get; init; }

    public required IReadOnlyList<LookupVm> SupplierOptions { get; init; }
}

public sealed class LookupVm
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}
