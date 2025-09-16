using System;
using System.Collections.Generic;

namespace NorthwindCharts.Database.AppDbContextModels;

public partial class CurrentProductList
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;
}
