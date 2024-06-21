using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AzraqSuiting.Models
{
    public class SaleDetails
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public decimal Discount { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Product")]
        [ForeignKey("Product")]
        public int? ProductId { get; set; }
        public Product Product { get; set; }



        [Display(Name = "Sales")]
        [ForeignKey("Sales")]
        [Required]
        public int SalesId { get; set; }
        public Sales Sales { get; set; }
    }
}