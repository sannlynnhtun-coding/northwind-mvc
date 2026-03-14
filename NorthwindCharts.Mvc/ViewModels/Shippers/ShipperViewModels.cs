using NorthwindCharts.Mvc.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace NorthwindCharts.Mvc.ViewModels.Shippers;

public sealed class ShipperListItemVm
{
    public int ShipperId { get; init; }

    public string CompanyName { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public int OrderCount { get; init; }
}

public sealed class ShipperIndexVm
{
    public required IReadOnlyList<ShipperListItemVm> Items { get; init; }

    public required PaginationVm Pagination { get; init; }
}

public sealed class ShipperFormVm
{
    [Required]
    [StringLength(40)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(24)]
    public string? Phone { get; set; }
}

public sealed class ShipperDetailsVm
{
    public int ShipperId { get; init; }

    public string CompanyName { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public int OrderCount { get; init; }
}
