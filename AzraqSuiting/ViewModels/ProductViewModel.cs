using AzraqSuiting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzraqSuiting.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }

        public List<Category> Category {  get; set; }
        public List<Brand> Brand { get; set; }

        public Category Categories { get; set; }
        public Brand Brands { get; set; }
    }
}