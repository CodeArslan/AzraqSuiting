using AzraqSuiting.Models;
using AzraqSuiting.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
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
        public ActionResult Index()
        {
            int? nextOrderNumber = GetNextOrderNumber();

            if (!nextOrderNumber.HasValue)
            {
                nextOrderNumber = 1001;
            }

            ViewBag.OrderNumber = nextOrderNumber;

            return View();
        }
        public ActionResult Edit(int SaleId)
        {
            ViewBag.SaleId = SaleId;

            return View();
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

                // Example data structure to return
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

        // Example method to fetch order info based on saleId
        private object GetOrderInfo(int saleId)
        {
            var sale = _dbContext.Sales
                                .Include(s => s.Customer) // Include Customer navigation property
                                .FirstOrDefault(s => s.Id == saleId);

            if (sale == null)
            {
                throw new Exception("Sale not found");
            }

            // Fetch order information including customer details
            var orderInfo = new
            {
                cumulativeDiscount=sale.Discount,
                orderNumber = sale.OrderNumber,
                orderDate = sale.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                customerName = sale.Customer.Name,
                customerPhone = sale.Customer.PhoneNumber
            };

            return orderInfo;
        }

        // Example method to fetch sale details based on saleId
        private object GetSalesInformation(int saleId)
        {
            // Replace with actual logic to fetch sale details
            var saleDetails = _dbContext.SaleDetails
                .Where(sd => sd.SalesId == saleId)
                .Select(sd => new
                {
                    Id = sd.Id,
                    ItemName = sd.Product != null ? sd.Product.Name : sd.ProductName,
                    UnitPrice = sd.UnitPrice,
                    Quantity = sd.Quantity,
                    Discount=sd.Discount,
                    Cost = (sd.UnitPrice * sd.Quantity) - sd.Discount
                })
                .ToList();

            return saleDetails;
        }
        [HttpPost]
        public ActionResult SaleDetails(SalesViewModel model)
        {
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
                        // Create a new customer
                        customer = new Customer
                        {
                            Name = model.Customer.customerName,
                            PhoneNumber = model.Customer.customerPhone
                        };
                        _dbContext.Customer.Add(customer);
                        _dbContext.SaveChanges(); 
                    }

                    // Create new sales record
                    var sale = new Sales
                    {
                        Discount=model.discount,
                        OrderNumber=model.orderNum,
                        Date = model.saleDate,
                        TotalAmount = CalculateTotalAmount(model.InvoiceDetails),
                        CustomerId = customer.Id
                    };
                    _dbContext.Sales.Add(sale);
                    _dbContext.SaveChanges(); 

                    foreach (var detail in model.InvoiceDetails)
                    {
                        if(detail.productId==null)
                        {
                            var saleDetail = new SaleDetails
                            {
                                Discount = detail.discount,
                                Quantity = detail.Quantity,
                                UnitPrice = detail.unitPrice,
                                ProductId =null,
                                ProductName=detail.ProductName,
                                SalesId = sale.Id
                            };
                            _dbContext.SaleDetails.Add(saleDetail);
                        }
                        else
                        {
                            var saleDetail = new SaleDetails
                            {
                                Discount=detail.discount,
                                Quantity = detail.Quantity,
                                UnitPrice = detail.unitPrice,
                                ProductId = detail.productId.Value,
                                SalesId = sale.Id
                            };
                            _dbContext.SaleDetails.Add(saleDetail);
                        }
                       
                        
                    }
                    _dbContext.SaveChanges();

                    return Json(new { success = true, message = "Sale details saved successfully", saleId = sale.Id });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred while saving the sale details. Please try again." });
                }
            }
            else
            {
                // Return validation errors if ModelState is not valid
                return Json(new { success = false, message = "Model State Not Valid" });
            }
        }

        // Helper method to calculate total amount based on invoice details
        private decimal CalculateTotalAmount(List<InvoiceDetailViewModel> invoiceDetails)
        {
            decimal total = 0;
            foreach (var detail in invoiceDetails)
            {
                total += detail.Quantity * detail.unitPrice - detail.discount;
            }
            return total;
        }
    }
}