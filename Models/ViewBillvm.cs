
using ShoppingApp.Data;

namespace ShoppingApp.Models
{
    public class ViewBillvm
    {
        public BillingTable bill { get; set; }
        public List<ItemsTable> items { get; set; }
    }
}
