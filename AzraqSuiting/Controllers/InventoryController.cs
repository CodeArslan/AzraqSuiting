using AzraqSuiting.Models;
using AzraqSuiting.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzraqSuiting.Controllers
{
    public class InventoryController : Controller
    {
        // GET: Inventory
        private ApplicationDbContext _dbContext;
        public InventoryController()
        {
            _dbContext = new ApplicationDbContext();
        }
        [HttpGet]
        public ActionResult GetProductById(int id)
        {
            var product = _dbContext.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return Json(product, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index()
        {
            var Category = _dbContext.Category.ToList();
            var Brand=_dbContext.Brand.ToList();
            var viewModel = new ProductViewModel
            {
                Category = Category,
                Brand = Brand,
            };
            return View(viewModel);
        }
        public ActionResult GetProductDetails()
        {
            // Fetch product names, codes, and IDs from database
            var productDetails = _dbContext.Product.Select(p => new { Name = p.Name, Code = p.Code, Id = p.Id }).ToList();
            return Json(productDetails, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ProductDetails(ProductViewModel model)
        {
            ModelState.Remove("Product.Id");
            ModelState.Remove("Product.AverageCost");

            try
            {
                if (ModelState.IsValid)
                {
                    string tempFolderPath = Server.MapPath("~/Temp/");
                    string permanentFolderPath = Server.MapPath("~/ProductBarcodes/");

                    if (!Directory.Exists(permanentFolderPath))
                    {
                        Directory.CreateDirectory(permanentFolderPath);
                    }

                    string newFileName = $"barcode_{model.Product.BarCode}.png";
                    string newFilePath = Path.Combine(permanentFolderPath, newFileName);
                    string tempImagePath = Path.Combine(tempFolderPath, Path.GetFileName(model.Product.BarcodeImagePath));

                    if (System.IO.File.Exists(tempImagePath))
                    {
                        System.IO.File.Copy(tempImagePath, newFilePath, true);
                    }

                    model.Product.BarcodeImagePath = Url.Content($"~/ProductBarcodes/{newFileName}");

                    if (model.Product.Id != 0)
                    {
                        var productInDb = _dbContext.Product.FirstOrDefault(m => m.Id == model.Product.Id);
                        if (productInDb != null)
                        {
                            productInDb.Name = model.Product.Name;
                            productInDb.Code = model.Product.Code;
                            productInDb.BarCode = model.Product.BarCode;
                            productInDb.Unit = model.Product.Unit;
                            productInDb.Description = model.Product.Description;
                            productInDb.Location = model.Product.Location;
                            productInDb.LatestSalePrice = model.Product.LatestSalePrice;
                            productInDb.LastPurchasePrice = model.Product.LastPurchasePrice;
                            productInDb.CurrentStock = model.Product.CurrentStock;
                            productInDb.DateAdded = DateTime.Now;
                            productInDb.CategoryId = model.Product.CategoryId;
                            productInDb.BrandId = model.Product.BrandId;
                            productInDb.CurrentValue = model.Product.CurrentValue;
                            productInDb.AverageCost = model.Product.AverageCost;
                            productInDb.MinimumStock = model.Product.MinimumStock;
                            productInDb.BarcodeImagePath = model.Product.BarcodeImagePath;
                            _dbContext.SaveChanges();
                            return Json(new { success = true, message = "Product details updated successfully." });
                        }
                    }
                    else
                    {
                        _dbContext.Product.Add(model.Product);
                        _dbContext.SaveChanges();
                        return Json(new { success = true, message = "Product Added successfully." });
                    }
                }
                else
                {
                    return Json(new { success = false, error = "An Error Occurred while Saving Product Details" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "An Error Occurred while Saving Product Details. Please try again later." });
            }

            return View(model);
        }
    }
}