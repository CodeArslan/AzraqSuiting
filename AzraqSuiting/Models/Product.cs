using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AzraqSuiting.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Product Code")]
        [Required]
        public int Code { get; set; }
        [Required]
        [Display(Name = "Barcode")]
        public string BarCode { get; set; }
        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Shelf/Location")]
        public string Location { get; set; }
        [Required]
        [Display(Name = "Latest Sale Price")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal LatestSalePrice { get; set; }
        [Required]
        [Display(Name = "Last Purchase Price")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal LastPurchasePrice { get; set; }

        [Display(Name = "Current Stock")]
        [Range(0, double.MaxValue, ErrorMessage = "Current stock cannot be negative")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal CurrentStock { get; set; }

        [Display(Name = "Meter Per Suit")]
        [Range(0, double.MaxValue, ErrorMessage = "Meter Per Suit cannot be negative")]
        [Required]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal MeterPerSuit { get; set; }

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; }=DateTime.Now;

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        [Display(Name = "Category")]
        [ForeignKey("Category")]
        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Display(Name = "Brand")]
        [ForeignKey("Brand")]
        public int? BrandId { get; set; }
        public Brand Brand { get; set; }
        [Display(Name = "Current Value")]
        [Range(0, double.MaxValue, ErrorMessage = "Current Value cannot be negative")]
        public decimal CurrentValue { get; set; }
        [Display(Name = "Average Cost")]
        [Range(0, double.MaxValue, ErrorMessage = "Average Cost cannot be negative")]
        public decimal AverageCost { get; set; }
        [Display(Name = "Minimum Stock Level")]
        [Range(0, double.MaxValue, ErrorMessage = "Minimun Stock Level cannot be negative")]
        public decimal MinimumStock { get; set; }

        [Required]
        [Display(Name = "Barcode Image Path")]
        public string BarcodeImagePath { get; set; }

    }
}