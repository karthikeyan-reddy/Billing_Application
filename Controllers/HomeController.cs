using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using ShoppingApp.data;
using ShoppingApp.Models;
using System.Data.Entity;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;

namespace ShoppingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            using (var context = new ShoppingApplicationContext())
            {
                try
                {
                    var vm = new HomePagevm();
                    vm.Items = context.ItemsTables
                                    .OrderBy(a => a.ItemName)
                                    .ToList();

                    vm.History = context.BillingTables
                                    .OrderByDescending(a => a.BillDateTime)
                                    .ToList();

                    var amountPaid = context.BillingTables.Select(a => a.AmountPaid).ToList().Sum();
                    var totalAmount = context.BillingTables.Select(a => a.TotalAmount).ToList().Sum();
                    vm.PayedAmount = Convert.ToInt32(amountPaid);
                    vm.UnPayedAmount = Convert.ToInt32(totalAmount - amountPaid);

                    return View(vm);
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Error");
                }
            }
        }

        [HttpPost]
        public ActionResult AddProduct(string itemName, string description, int price, int stocks)
        {
            using (var context = new ShoppingApplicationContext())
            {
                bool isProductExists = context.ItemsTables.First(a => a.ItemName.Equals(itemName)) != null;
                var item = new ItemsTable();
                item.Description = description;
                item.ItemName = itemName;
                item.Price = price;
                item.TotalStock = stocks;
                item.ItemNumber = isProductExists ? Guid.NewGuid() : context.ItemsTables.FirstOrDefault(a => a.ItemName == itemName).ItemNumber;
                item.StockAvailable = stocks;
                try
                {
                    if (!isProductExists)
                    {
                        context.ItemsTables.Update(item);
                        context.SaveChanges();
                    }
                    else
                    {
                        context.ItemsTables.Add(item);
                        context.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Error");
                }
            }
        }

        [HttpPost]
        public ActionResult AddInvoice(string customerName, string mobileNumber, string emailId, string address, string products, string amount)
        {
            var purchasedProducts = new StringBuilder();
            using (var context = new ShoppingApplicationContext())
            {
                foreach (var product in products.Split(','))
                {
                    purchasedProducts.AppendLine(context.ItemsTables.First(a => a.ItemNumber.ToString() == product).ItemName.ToString() + ",");
                }

                var bill = new BillingTable()
                {
                    CustomerAddress = address,
                    CustomerName = customerName,
                    CustomerEmailId = emailId,
                    CustomerMobileNumber = Convert.ToInt64(mobileNumber),
                    TotalAmount = Convert.ToInt32(amount),
                    ItemsPurchased = purchasedProducts.ToString().TrimEnd(','),
                    BillDateTime = DateTime.Now,
                    BillNumber = Guid.NewGuid()
                };
                try
                {
                    context.BillingTables.Add(bill);
                    context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Error");
                }
            }
        }

        [HttpPost]
        public ActionResult AddBill(string customerName, string mobileNumber, string emailId, string address, string products, string amount, int billType, int AmountPaid, int PaymentMode)
        {
            using (var context = new ShoppingApplicationContext())
            {
                var purchasedProducts = new StringBuilder();
                foreach (var product in products.Split(','))
                {
                    purchasedProducts.AppendLine(context.ItemsTables.First(a => a.ItemNumber.ToString() == product).ItemName.ToString());
                }
                var bill = new BillingTable()
                {
                    CustomerAddress = address,
                    CustomerName = customerName,
                    CustomerEmailId = emailId,
                    CustomerMobileNumber = Convert.ToInt64(mobileNumber),
                    TotalAmount = Convert.ToInt32(amount),
                    ItemsPurchased = purchasedProducts.ToString().Trim(','),
                    BillDateTime = DateTime.Now,
                    BillNumber = Guid.NewGuid(),
                    PaymentMode = PaymentMode,
                    AmountPaid = AmountPaid,
                    BillType = billType
                };

                try
                {
                    context.BillingTables.Add(bill);
                    context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Error");
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[HttpPost]
        //public ActionResult AddBill(string customerName, string mobileNumber, string emailId, string address, List<BillProducts> products, string amount, int billType, int AmountPaid, int PaymentMode)
        //{
        //    using (var context = new ShoppingApplicationContext())
        //    {
        //        var purchasedProducts = new StringBuilder();

        //        foreach (var product in products)
        //        {
        //            purchasedProducts.AppendLine($"{product.Product} - {product.Quantity}");
        //        }

        //        var bill = new BillingTable()
        //        {
        //            CustomerAddress = address,
        //            CustomerName = customerName,
        //            CustomerEmailId = emailId,
        //            CustomerMobileNumber = Convert.ToInt64(mobileNumber),
        //            TotalAmount = Convert.ToInt32(amount),
        //            ItemsPurchased = purchasedProducts.ToString().Trim(),
        //            BillDateTime = DateTime.Now,
        //            BillNumber = Guid.NewGuid(),
        //            PaymentMode = PaymentMode,
        //            AmountPaid = AmountPaid,
        //            BillType = billType
        //        };

        //        try
        //        {
        //            context.BillingTables.Add(bill);
        //            context.SaveChanges();
        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception ex)
        //        {
        //            return RedirectToAction("Error");
        //        }
        //    }
        //}
    
    }
}
