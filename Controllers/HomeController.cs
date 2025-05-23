using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Data;
using ShoppingApp.Models;
using System.Data.Entity;
using System.Diagnostics;
using System.Text;
using Document = iTextSharp.text.Document;
using Microsoft.AspNetCore.Authorization;

namespace ShoppingApp.Controllers
{
    [Authorize]
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

                    // Go through the bill items and check the items and get the dictionary of items and quantity. 

                    var frequentDict = new Dictionary<string, int>();

                    foreach (var bill in vm.History.ToList())
                    {
                        var items = bill.ItemsPurchased.Split(',');

                        foreach (var item in items)
                        {
                            if (vm.Items.Any(a => a.ItemName == item.Split('-')[0]))
                            {
                                if (frequentDict.ContainsKey(item.Split('-')[0]))
                                {
                                    frequentDict[item.Split('-')[0]] += Convert.ToInt32(item.Split('-')[1]);
                                }
                                else { frequentDict.Add(item.Split('-')[0], Convert.ToInt32(item.Split('-')[1])); }
                            }
                        }
                    }

                    var top5 = frequentDict.OrderByDescending(a => a.Value).Take(5).Select(a => a.Key).ToList();

                    vm.FrequentItems = context.ItemsTables.Where(a => top5.Contains(a.ItemName)).ToList();

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
        public ActionResult AddProduct(string itemName, string description, decimal price, int stocks, string itemNumber)
        {
            using (var context = new ShoppingApplicationContext())
            {
                var item = context.ItemsTables.FirstOrDefault(a => a.ItemName == itemName);

                if (item != null)
                {
                    item.Description = description;
                    item.Price = price;
                    item.TotalStock = stocks;
                    item.StockAvailable = stocks;
                    context.ItemsTables.Update(item);
                }
                else
                {
                    item = new ItemsTable();
                    item.Description = description;
                    item.ItemName = itemName;
                    item.Price = price;
                    item.TotalStock = stocks;
                    item.ItemNumber = Guid.NewGuid();
                    item.StockAvailable = stocks;
                    context.ItemsTables.Add(item);
                }

                try
                {
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
                var bill = new BillingTable()
                {
                    CustomerAddress = address,
                    CustomerName = customerName,
                    CustomerEmailId = emailId,
                    CustomerMobileNumber = long.Parse(mobileNumber),
                    TotalAmount = Convert.ToInt32(amount.Replace('₹', ' ')),
                    ItemsPurchased = products.Trim(','),
                    BillDateTime = DateTime.Now,
                    BillNumber = Guid.NewGuid(),
                    PaymentMode = PaymentMode,
                    AmountPaid = Convert.ToDecimal(AmountPaid),
                    BillType = billType
                };

                // Update Stock Available. 

                var items = products.Trim(' ').Split(',');
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var itemQuantity = item.Split('-');
                        var itemToBeUpdated = context.ItemsTables.First(a => a.ItemName == itemQuantity[0].ToString());
                        itemToBeUpdated.StockAvailable = itemToBeUpdated.StockAvailable - Convert.ToInt32(itemQuantity[1]);
                        context.ItemsTables.Update(itemToBeUpdated);
                    }
                }

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

        [HttpGet]
        public FileResult GenerateBill(string itemNumber)
        {
            // Create a new PDF document
            using (var doc = new Document())
            {

                // Add items to the table
                var bill = GetBill(itemNumber);
                var items = bill.bill.ItemsPurchased.Split(',');

                PdfWriter.GetInstance(doc, new FileStream(bill.bill.BillNumber + ".pdf", FileMode.Create));

                // Set the page size and margins
                doc.SetPageSize(PageSize.A4);
                doc.SetMargins(36, 36, 36, 36);

                // Create a new page

                doc.Open();

                // Add a banner image
                var bannerImage = Image.GetInstance("D:\\visual studio\\ShoppingApp\\wwwroot\\Images\\Banner1.png");
                bannerImage.ScaleToFitLineWhenOverflow = true;
                doc.Add(bannerImage);

                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph("\n"));

                // Add a header section

                var headerTable = new PdfPTable(2);
                headerTable.TotalWidth = 500;
                headerTable.LockedWidth = true;
                headerTable.SetWidths(new float[] { 1, 2 });
                headerTable.AddCell(new PdfPCell(new Phrase("Date: ")) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                headerTable.AddCell(new PdfPCell(new Phrase(bill.bill.BillDateTime.ToString())) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                doc.Add(headerTable);

                headerTable = new PdfPTable(2);
                headerTable.TotalWidth = 500;
                headerTable.LockedWidth = true;
                headerTable.SetWidths(new float[] { 1, 2 });
                headerTable.AddCell(new PdfPCell(new Phrase("Invoice Number: ")) { Border = Rectangle.NO_BORDER });
                headerTable.AddCell(new PdfPCell(new Phrase(itemNumber)) { Border = Rectangle.NO_BORDER });
                doc.Add(headerTable);




                // Add a spacer

                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph("\n"));

                // Add an items section
                var itemsTable = new PdfPTable(4);
                itemsTable.TotalWidth = 500;
                itemsTable.LockedWidth = true;
                itemsTable.SetWidths(new float[] { 1, 2, 1, 1 });
                itemsTable.AddCell(new PdfPCell(new Phrase("Item")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                itemsTable.AddCell(new PdfPCell(new Phrase("Quantity")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                itemsTable.AddCell(new PdfPCell(new Phrase("UnitPrice")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                itemsTable.AddCell(new PdfPCell(new Phrase("TotalPrice")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                //itemsTable.AddCell(new PdfPCell(new Phrase("Discount")));


                foreach (var item in items)
                {
                    var cItem = item.Split('-');
                    var unitPrice = bill.items.Where(a => a.ItemName == cItem[0]).First().Price;
                    var totalPrice = CalculatePrice(unitPrice, int.Parse(cItem[1]));

                    itemsTable.AddCell(new PdfPCell(new Phrase(cItem[0])));
                    itemsTable.AddCell(new PdfPCell(new Phrase(cItem[1])));
                    itemsTable.AddCell(new PdfPCell(new Phrase(unitPrice.ToString())));
                    itemsTable.AddCell(new PdfPCell(new Phrase(totalPrice.ToString())));
                    //itemsTable.AddCell(new PdfPCell(new Phrase(0)) { Border = Rectangle.NO_BORDER });
                }

                doc.Add(itemsTable);

                // Add a footer section

                var paymentMode = bill.bill.PaymentMode switch
                {
                    1 => "Cash",
                    2 => "Online",
                    _ => "Card"
                };


                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph("\n"));

                var footerTable = new PdfPTable(1);
                footerTable.TotalWidth = 500;
                footerTable.LockedWidth = true;
                footerTable.AddCell(new PdfPCell(new Phrase("Total: " + bill.bill.TotalAmount)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                footerTable.AddCell(new PdfPCell(new Phrase("Amount Paid: " + bill.bill.AmountPaid)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                footerTable.AddCell(new PdfPCell(new Phrase("Payment Mode: " + paymentMode)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                doc.Add(footerTable);


                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph("\n"));

                var chunk = new Chunk("Customer Details", FontFactory.GetFont("Arial", 12, Font.BOLD));
                doc.Add(new Paragraph(chunk));
                doc.Add(new Paragraph(new Phrase("Name: " + bill.bill.CustomerName)));
                doc.Add(new Paragraph(new Phrase("Address: " + bill.bill.CustomerAddress)));
                doc.Add(new Paragraph(new Phrase("Phone: " + bill.bill.CustomerMobileNumber)));


                doc.Close();

                return File(System.IO.File.ReadAllBytes(bill.bill.BillNumber + ".pdf"), "application/pdf", bill.bill.BillNumber + ".pdf");

            }
        }

        public static decimal CalculatePrice(decimal unitPrice, int Quantity) { return unitPrice * Quantity; }

        [HttpGet]
        public ViewBillvm GetBill(string itemNumber)
        {
            var vm = new ViewBillvm();

            using (var db = new ShoppingApplicationContext())
            {
                vm.bill = db.BillingTables.Where(b => b.BillNumber == Guid.Parse(itemNumber)).First();
                vm.items = db.ItemsTables.ToList();
            }
            return vm;
        }


        /*
        public FileResult GenerateBill(string itemNumber)
        {
            
            var item = new BillingTable();
            using (var context = new ShoppingApplicationContext())
            {
                item = context.BillingTables.FirstOrDefault(a => a.BillNumber.ToString() == itemNumber);
            }

            Document doc = new Document(); 
            PdfWriter.GetInstance(doc, new FileStream(item.BillNumber + ".pdf", FileMode.Create));
            doc.Open();

            // Add headers to the document
            Paragraph header = new Paragraph("Date: " + item.BillDateTime);

            doc.Add(header);

            header = new Paragraph("Invoice Number: " + item.BillNumber.ToString());
            doc.Add(header);

            /// Add image to the document
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance("path_to_your_image.jpg");
            image.ScaleToFit(100, 100);
            doc.Add(image);/

            // Create a new table
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;

            // Add headers to the table
            PdfPCell headerCell = new PdfPCell(new Phrase("Product Name"));
            headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(headerCell);

            headerCell = new PdfPCell(new Phrase("Quantity"));
            headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(headerCell);

            headerCell = new PdfPCell(new Phrase("Unit Price"));
            headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(headerCell);

            headerCell = new PdfPCell(new Phrase("Total Price"));
            headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(headerCell);

            headerCell = new PdfPCell(new Phrase("Discount"));
            headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(headerCell);


            //Extract the products Added. 
            var products = item.ItemsPurchased.Split(',');

            foreach (var product in products)
            {
                var productName = product.Split('-')[0];
                var productQty = product.Split('-')[1];
                var productDetails = new ItemsTable();

                using (var context = new ShoppingApplicationContext())
                {
                    productDetails = context.ItemsTables.FirstOrDefault(a => a.ItemName == productName);
                }

                PdfPCell cell = new PdfPCell(new Phrase(productName));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(productQty));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(productDetails.Price.ToString()));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase((productDetails.Price * Convert.ToInt32(productQty)).ToString()));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(0));
                table.AddCell(cell);
                
            }
            // Add the table to the document
            doc.Add(table);

            // Add footer to the document
            Paragraph footer = new Paragraph($"Total Amount: ₹{item.TotalAmount.ToString()}");
            doc.Add(footer);

            footer = new Paragraph($"Amount Paid: ₹{item.AmountPaid.ToString()}");
            doc.Add(footer);

            footer = new Paragraph($"Payment Mode: {item.PaymentMode}");
            doc.Add(footer);

            footer = new Paragraph("Customer Details:");
            doc.Add(footer);

            footer = new Paragraph($"Name: {item.CustomerName}");
            doc.Add(footer);

            footer = new Paragraph($"Address: {item.CustomerAddress}");
            doc.Add(footer);

            footer = new Paragraph($"Phone: {item.CustomerMobileNumber}");
            doc.Add(footer);

            // Close the document
            doc.Close();


            return File(System.IO.File.ReadAllBytes(item.BillNumber + ".pdf"), "application/pdf", item.BillNumber + ".pdf");

        }
        */
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
