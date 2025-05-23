using System;
using System.Collections.Generic;

namespace ShoppingApp.Data;

public partial class BilledProduct
{
    public Guid BillId { get; set; }

    public string? ProductName { get; set; }

    public int? Quantity { get; set; }
}
