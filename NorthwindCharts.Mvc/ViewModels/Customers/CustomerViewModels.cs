using NorthwindCharts.Mvc.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace NorthwindCharts.Mvc.ViewModels.Customers;

public sealed class CustomerListItemVm
{
    public string CustomerId { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string? ContactName { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    public string? Phone { get; init; }

    public int OrderCount { get; init; }
}

public sealed class CustomerIndexVm
{
    public required IReadOnlyList<CustomerListItemVm> Items { get; init; }

    public required PaginationVm Pagination { get; init; }
}

public sealed class CustomerFormVm
{
    [Required]
    [StringLength(5, MinimumLength = 5)]
    public string CustomerId { get; set; } = string.Empty;

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
}

public sealed class CustomerOrderSummaryVm
{
    public int OrderId { get; init; }

    public DateTime? OrderDate { get; init; }

    public DateTime? ShippedDate { get; init; }

    public decimal? Freight { get; init; }
}

public sealed class CustomerDetailsVm
{
    public string CustomerId { get; init; } = string.Empty;

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

    public required IReadOnlyList<CustomerOrderSummaryVm> RecentOrders { get; init; }
}
