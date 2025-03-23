using ShoppingApp.data;

namespace ShoppingApp.Models
{
    public class HomePagevm
    {
        public BillingTableExtended Bill { get; set; }
        public ItemsTable Item { get; set; }
        public List<BillingTable> History { get; set; }
        public List<ItemsTable> Items { get; set; }
        public List<ItemsTable> FrequentItems { get;}
        public int PayedAmount { get; set; }
        public int UnPayedAmount { get; set; }
    }
    public class BillingTableExtended : BillingTable
    {
        public List<BillProducts> products { get; set; }
    }
}
