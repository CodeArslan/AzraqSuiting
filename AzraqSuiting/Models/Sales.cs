using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AzraqSuiting.Models
{
    public class Sales
    {
        public int Id { get; set; }
        [Display(Name = "Sale Date")]

        public DateTime Date { get; set; }
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }

         public int OrderNumber { get; set; }

        [Display(Name = "Customer")]
        [ForeignKey("Customer")]
        [Required]
        public int? CustomerId { get; set; }
        public Customer Customer { get; set; }
        [Display(Name = "Customer")]
        public string Instructions { get; set; }


    }
}