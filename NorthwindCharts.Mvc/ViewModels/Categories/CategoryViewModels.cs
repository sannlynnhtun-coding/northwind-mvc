using NorthwindCharts.Mvc.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace NorthwindCharts.Mvc.ViewModels.Categories;

public sealed class CategoryListItemVm
{
    public int CategoryId { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int ProductCount { get; init; }
}

public sealed class CategoryIndexVm
{
    public required IReadOnlyList<CategoryListItemVm> Items { get; init; }

    public required PaginationVm Pagination { get; init; }
}

public sealed class CategoryFormVm
{
    [Required]
    [StringLength(15)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }
}

public sealed class CategoryDetailsVm
{
    public int CategoryId { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int ProductCount { get; init; }
}
