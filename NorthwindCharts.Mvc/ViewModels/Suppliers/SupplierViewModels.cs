using NorthwindCharts.Mvc.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace NorthwindCharts.Mvc.ViewModels.Suppliers;

public sealed class SupplierListItemVm
{
    public int SupplierId { get; init; }

    public string CompanyName { get; init; } = string.Empty;

    public string? ContactName { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    public string? Phone { get; init; }

    public int ProductCount { get; init; }
}

public sealed class SupplierIndexVm
{
    public required IReadOnlyList<SupplierListItemVm> Items { get; init; }

    public required PaginationVm Pagination { get; init; }
}

public sealed class SupplierFormVm
{
    [Required]
    [StringLength(40)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? ContactName { get; set; }

    [StringLength(30)]
    public string? ContactTitle { get; set; }

    [StringLength(60)]
    public string? Address { get; set; }

    [StringLength(15)]
    public string? City { get; set; }

    [StringLength(15)]
    public string? Region { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }

    [StringLength(15)]
    public string? Country { get; set; }

    [StringLength(24)]
    public string? Phone { get; set; }

    [StringLength(24)]
    public string? Fax { get; set; }

    public string? HomePage { get; set; }
}

public sealed class SupplierDetailsVm
{
    public int SupplierId { get; init; }

    public string CompanyName { get; init; } = string.Empty;

    public string? ContactName { get; init; }

    public string? ContactTitle { get; init; }

    public string? Address { get; init; }

    public string? City { get; init; }

    public string? Region { get; init; }

    public string? PostalCode { get; init; }

    public string? Country { get; init; }

    public string? Phone { get; init; }

    public string? Fax { get; init; }

    public int ProductCount { get; init; }
}
