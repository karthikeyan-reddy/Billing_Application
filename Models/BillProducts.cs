using ShoppingApp.Data;

namespace ShoppingApp.Models
{
    public class BillProducts
    { 
        public string Product { get; set; }
        public int Quantity { get; set; }
    }

    public class BillRequest
    {
        public string CustomerName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        public string Address { get; set; }
        public string Amount { get; set; }
        public int BillType { get; set; }
        public int AmountPaid { get; set; }
        public int PaymentMode { get; set; }
        public List<BillProducts> Products { get; set; }
    }
}
