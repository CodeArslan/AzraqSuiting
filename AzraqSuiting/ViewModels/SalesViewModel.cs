using AzraqSuiting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzraqSuiting.ViewModels
{
    public class SalesViewModel
    {
        public int SaleId { get; set; }
        public DateTime saleDate { get; set; } // Sale Date
        public int orderNum { get; set; }
        public decimal discount { get; set; }
        public CustomerViewModel Customer { get; set; }
        public List<InvoiceDetailViewModel> InvoiceDetails { get; set; }


    }
    public class CustomerViewModel
    {
        public int? customerId { get; set; } // Nullable CustomerId
        public string customerName { get; set; } // CustomerName for display

        public string customerPhone { get; set; } // CustomerPhone

    }

    public class InvoiceDetailViewModel
    {
        public decimal totalAmount { get; set; } // Total Amount

        public int? productId { get; set; } // Nullable ProductId
        public string ProductName { get; set; } // ProductName
        public int Quantity { get; set; } // Quantity
        public decimal unitPrice { get; set; } // UnitPrice
        public decimal discount { get; set; } // Discount

    }
}