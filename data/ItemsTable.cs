using System;
using System.Collections.Generic;

namespace ShoppingApp.Data;

public partial class ItemsTable
{
    public Guid ItemNumber { get; set; }

    public string ItemName { get; set; } = null!;

    public string? ItemImage { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public int StockAvailable { get; set; }

    public int? TotalStock { get; set; }
}
