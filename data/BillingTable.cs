using System;
using System.Collections.Generic;

namespace ShoppingApp.Data;

public partial class BillingTable
{
    public Guid BillNumber { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerAddress { get; set; } = null!;

    public DateTime BillDateTime { get; set; }

    public decimal TotalAmount { get; set; }

    public int? BillType { get; set; }

    public int? PaymentMode { get; set; }

    public decimal? AmountPaid { get; set; }

    public long CustomerMobileNumber { get; set; }

    public string CustomerEmailId { get; set; } = null!;

    public bool? MessageStatus { get; set; }

    public string ItemsPurchased { get; set; } = null!;
}
