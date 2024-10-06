using AzraqSuiting.Models;
using AzraqSuiting.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
namespace AzraqSuiting.Controllers
{
    public class SalesController : Controller
    {
        // GET: Sales
        private ApplicationDbContext _dbContext;
        public SalesController()
        {
            _dbContext = new ApplicationDbContext();
        }
        public ActionResult Add()
        {
            int? nextOrderNumber = GetNextOrderNumber();

            if (!nextOrderNumber.HasValue)
            {
                nextOrderNumber = 1001;
            }

            ViewBag.OrderNumber = nextOrderNumber;

            return View();
        }
        public ActionResult Details()
        {

            return View();
        }
        public ActionResult Edit(int? SaleId)
        {
            if (SaleId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.SaleId = SaleId;
            return View();
        }
        public ActionResult GetSalesData()
        {
            var sales = _dbContext.Sales.Include(s => s.Customer).ToList();

            var data = sales.Select(s => new
            {
                OrderNumber = s.OrderNumber,
                Customer = s.Customer.Name,
                Date = s.Date.ToString("yyyy-MM-dd"), // Format date as needed
                Discount = s.Discount,
                TotalAmount = s.TotalAmount,
                cashPaid=s.CashPaid,
                balance=s.Balance,
                Id = s.Id
            }).ToList();

            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteSales(int id)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var sale = _dbContext.Sales.FirstOrDefault(s => s.Id == id);
                    if (sale == null)
                    {
                        return Json(new { success = false, message = "Sale not found." });
                    }

                    var saleDetails = _dbContext.SaleDetails.Where(sd => sd.SalesId == id).ToList();

                    foreach (var detail in saleDetails)
                    {
                        var product = _dbContext.Product.FirstOrDefault(p => p.Id == detail.ProductId);
                        if (product != null)
                        {
                            decimal meterstoAdd = detail.Quantity * product.MeterPerSuit;
                            product.CurrentStock += meterstoAdd;
                        }
                    }

                    _dbContext.SaleDetails.RemoveRange(saleDetails);

                    _dbContext.Sales.Remove(sale);
                    _dbContext.SaveChanges();

                    transaction.Commit();

                    return Json(new { success = true, message = "Sale deleted and inventory updated successfully." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "An error occurred while deleting the sale and updating the inventory." });
                }
            }
        }


        private int? GetNextOrderNumber()
        {
            int? nextOrderNumber = null; 

            try
            {
                    var lastOrder = _dbContext.Sales.OrderByDescending(s => s.OrderNumber).FirstOrDefault();

                    if (lastOrder != null)
                    {
                        nextOrderNumber = lastOrder.OrderNumber + 1;
                    }
                    else
                    {
                        nextOrderNumber = 1001;
                    }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching next order number: " + ex.Message);
                nextOrderNumber = 1001;
            }

            return nextOrderNumber;
        }


        [HttpGet]
        public ActionResult GetSaleDetails(int saleId)
        {
            try
            {
                var orderInfo = GetOrderInfo(saleId); 
                var saleDetails = GetSalesInformation(saleId); 

                var data = new
                {
                    orderInfo = orderInfo,
                    saleDetails = saleDetails
                };

                return Json(data, JsonRequestBehavior.AllowGet); // Return JSON data to client-side
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                return Json(new { success = false, message = "Error fetching sale details: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private object GetOrderInfo(int saleId)
        {
            var sale = _dbContext.Sales
                                .Include(s => s.Customer) 
                                .FirstOrDefault(s => s.Id == saleId);

            if (sale == null)
            {
                throw new Exception("Sale not found");
            }

            var orderInfo = new
            {
                cumulativeDiscount=sale.Discount,
                orderNumber = sale.OrderNumber,
                orderDate = sale.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                customerName = sale.Customer.Name,
                customerPhone = sale.Customer.PhoneNumber,
                customerId=sale.Customer.Id,
                instructions=sale.Instructions,
                cashPaid=sale.CashPaid,
                balance=sale.Balance
            };

            return orderInfo;
        }

        private object GetSalesInformation(int saleId)
        {
            var saleDetails = _dbContext.SaleDetails
                .Where(sd => sd.SalesId == saleId)
                .Select(sd => new
                {
                    Id = sd.Id,
                    ItemName = sd.Product != null ? sd.Product.Name : sd.ProductName,
                    UnitPrice = sd.UnitPrice,
                    Quantity = sd.Quantity,
                    Discount=sd.Discount,
                    Cost = (sd.UnitPrice * sd.Quantity) - sd.Discount,
                    
                })
                .ToList();

            return saleDetails;
        }
        [HttpPost]
        public ActionResult SaleDetails(SalesViewModel model)
        {
            ModelState.Remove("SaleId");
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if customer exists or create a new one
                    Customer customer;
                    if (model.Customer.customerId.HasValue)
                    {
                        customer = _dbContext.Customer.Find(model.Customer.customerId.Value);
                        if (customer == null)
                        {
                            return Json(new { success = false, message = "Invalid Customer selection." });
                        }
                    }
                    else
                    {
                        customer = new Customer
                        {
                            Name = model.Customer.customerName,
                            PhoneNumber = model.Customer.customerPhone
                        };
                        _dbContext.Customer.Add(customer);
                        _dbContext.SaveChanges();
                    }

                   
                    Sales sale;
                    if (model.SaleId>0)
                    {
                        sale = _dbContext.Sales.Find(model.SaleId);
                        if (sale == null)
                        {
                            return Json(new { success = false, message = "Sale record not found." });
                        }
                        sale.Discount = model.discount;
                        sale.OrderNumber = model.orderNum;
                        sale.Date = model.saleDate;
                        sale.TotalAmount = CalculateTotalAmount(model.InvoiceDetails);
                        sale.CustomerId = customer.Id;
                        sale.Instructions = model.instructions;
                        sale.CashPaid = model.CashPaid;
                        sale.Balance = model.Balance;
                      
                        var existingSaleDetails = _dbContext.SaleDetails.Where(sd => sd.SalesId == sale.Id);
                        _dbContext.SaleDetails.RemoveRange(existingSaleDetails);
                    }
                    else
                    {
                        sale = new Sales
                        {
                            Discount = model.discount,
                            OrderNumber = model.orderNum,
                            Date = model.saleDate,
                            TotalAmount = CalculateTotalAmount(model.InvoiceDetails),
                            CustomerId = customer.Id,
                            Instructions= model.instructions,
                            CashPaid=model.CashPaid,
                            Balance=model.Balance
                        };
                        _dbContext.Sales.Add(sale);
                    }
                    
                    _dbContext.SaveChanges();

                    foreach (var detail in model.InvoiceDetails)
                    {
                        var saleDetail = new SaleDetails
                        {
                            Discount = detail.discount,
                            Quantity = detail.Quantity,
                            UnitPrice = detail.unitPrice,
                            ProductId = detail.productId,
                            ProductName = detail.productId == null ? detail.ProductName : null, // Only set ProductName if productId is null
                            SalesId = sale.Id
                        };
                        _dbContext.SaleDetails.Add(saleDetail);
                       
                    }
                    _dbContext.SaveChanges();
                    BackgroundJob.Enqueue(() => UpdateAverageCostAndInventoryAsync(model.InvoiceDetails));
                    model.Customer.customerName=customer.Name;
                    model.Customer.customerPhone=customer.PhoneNumber;
                    
                    var printerService = new PrinterService(); 
                    printerService.PrintInvoice(model);
                    return Json(new { success = true, message = "Sale details saved successfully", saleId = sale.Id });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred while saving the sale details. Please try again." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Model State Not Valid" });
            }
        }


        private decimal CalculateTotalAmount(List<InvoiceDetailViewModel> invoiceDetails)
        {
            decimal total = 0;
            foreach (var detail in invoiceDetails)
            {
                total += detail.Quantity * detail.unitPrice - detail.discount;
            }
            return total;
        }
        public async Task UpdateAverageCostAndInventoryAsync(List<InvoiceDetailViewModel> invoiceDetails)
        {
            foreach (var detail in invoiceDetails)
            {
                if (detail.productId != null)
                {
                    var product = await _dbContext.Product.FindAsync(detail.productId);
                    if (product != null)
                    {
                        var salesDetailsForProduct = await _dbContext.SaleDetails
                                                            .Where(sd => sd.ProductId == detail.productId)
                                                            .ToListAsync();
                        decimal totalCost = 0;
                        int totalQuantity = 0;

                        foreach (var saleDetail in salesDetailsForProduct)
                        {
                            totalCost += saleDetail.UnitPrice * saleDetail.Quantity;
                            totalQuantity += saleDetail.Quantity;
                        }

                        totalCost += detail.unitPrice * detail.Quantity;
                        totalQuantity += detail.Quantity;

                        decimal newAverageCost = totalCost / totalQuantity;

                        product.AverageCost = newAverageCost;

                        decimal metersToDeduct = detail.Quantity * product.MeterPerSuit;
                        product.CurrentStock -= metersToDeduct;

                        _dbContext.Entry(product).State = EntityState.Modified;
                    }
                }
            }

            await _dbContext.SaveChangesAsync(); 
        }



        //edit cases


        [HttpGet]
        public ActionResult GetSaleDetailsForEdit(int saleId)
        {
            var saleDetails = _dbContext.SaleDetails
                .Where(sd => sd.SalesId == saleId)
                .Select(sd => new
                {
                    Id = sd.Id,
                    ProductId = sd.ProductId,
                    ProductName = sd.Product != null ? sd.Product.Name : sd.ProductName,
                    UnitPrice = sd.UnitPrice,
                    Quantity = sd.Quantity,
                    Discount = sd.Discount,
                    Cost = (sd.UnitPrice * sd.Quantity) - sd.Discount,
                    IsNew = !sd.ProductId.HasValue
                })
                .ToList();

            return Json(new { saleDetails }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateSalesField(int id, string fieldName, decimal newValue)
        {
            try
            {
                var sale = _dbContext.Sales.Find(id);
                if (sale == null)
                {
                    return Json(new { success = false, message = "Sale not found." });
                }

                switch (fieldName)
                {
                    case "cashPaid":
                        sale.CashPaid = newValue;
                        break;
                    case "balance":
                        sale.Balance = newValue;
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid field name." });
                }

                _dbContext.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}