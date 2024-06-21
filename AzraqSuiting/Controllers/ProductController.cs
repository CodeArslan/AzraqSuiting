using AzraqSuiting.Models;
using AzraqSuiting.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AzraqSuiting.HelperMethods;
using System.Data.Entity;
using System.IO;
using ZXing;
namespace AzraqSuiting.Controllers
{
    public class ProductController : Controller
    {

        // GET: Product
        private ApplicationDbContext _dbContext;
        public ProductController()
        {
            _dbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            var Category = _dbContext.Category.ToList();
            var Brand = _dbContext.Brand.ToList();
            var nextProductCode = GetNextProductCode();

           

            var viewModel = new ProductViewModel
            {
                
                Category = Category,
                Brand = Brand,
                Product = new Product
                {
                    Code = nextProductCode
                }
            };
            return View(viewModel);
        }
        private int GetNextProductCode()
        {
            // Fetch the latest product
            var latestProduct = _dbContext.Product
                .OrderByDescending(p => p.Code)
                .FirstOrDefault();

            // Determine the next product code
            if (latestProduct == null)
            {
                // Default value in case there are no products
                return 1000;
            }

            // Increment the latest product code
            return latestProduct.Code + 1;
        }
        public ActionResult GetProductData()
        {
            var products = _dbContext.Product.Include(c=>c.Category).Include(b=>b.Brand).ToList();
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetProductDataForDropdown(string q)
        {
            var products = _dbContext.Product
                .Where(p => p.Name.Contains(q))
                .Select(p => new {
                    id = p.Id,
                    text = p.Name,
                    price = p.LatestSalePrice
                })
                .ToList();

            return Json(new { results = products }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GenerateBarcode()
        {
            string barcodeContent = Guid.NewGuid().ToString("N").Substring(0, 10);

            Bitmap barcodeImage = BarcodeHelper.GenerateBarcode(barcodeContent);
            byte[] barcodeImageBytes = BarcodeHelper.ConvertToByteArray(barcodeImage);
            // Temporary folder path
            string tempFolderPath = Server.MapPath("~/Temp/");

           
            // Save image to temporary folder
            string imagePath = Path.Combine(tempFolderPath, "barcode.png");
            barcodeImage.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

            // Return JSON response with barcode content and image path
            return Json(new
            {
                BarcodeContent = barcodeContent,
                BarcodeImagePath = Url.Content("~/Temp/barcode.png")
            }, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public ActionResult GetProductById(int id)
        {
            var product = _dbContext.Product.FirstOrDefault(m=>m.Id == id);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" }, JsonRequestBehavior.AllowGet);
            }

            return Json(product, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int id)
        {
            var productInDb = _dbContext.Product.SingleOrDefault(d => d.Id == id);
            if (productInDb == null)
            {
                return Json(new { success = false, message = "Product Doesnot Found" });
            }

            else
            {
                _dbContext.Product.Remove(productInDb);
                _dbContext.SaveChanges();
                return Json(new { success = true, message = "Product Deleted Succesfully" });
            }
        }
        public ActionResult GetCategoryData()
        {
            var category = _dbContext.Category
                                     .OrderByDescending(c => c.Id)
                                     .ToList();
            return Json(category, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteCategory(int id)
        {
            var categoryInDb = _dbContext.Category.SingleOrDefault(d => d.Id == id);
            if (categoryInDb == null)
            {
                return Json(new { success = false, message = "Category Doesnot Found" });
            }

            else
            {
                _dbContext.Category.Remove(categoryInDb);
                _dbContext.SaveChanges();
                return Json(new { success = true, message = "Category Deleted Succesfully" });
            }
        }

        public ActionResult GetBrandData()
        {
            var brands = _dbContext.Brand
                                  .OrderByDescending(b => b.Id)
                                  .ToList();
            return Json(brands, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteBrand(int id)
        {
            var brandInDb = _dbContext.Brand.SingleOrDefault(b => b.Id == id);
            if (brandInDb == null)
            {
                return Json(new { success = false, message = "Brand not found." });
            }

            // Delete all related products first
            var productsInDb = _dbContext.Product.Where(p => p.BrandId == id).ToList();
            foreach (var product in productsInDb)
            {
                product.BrandId = null;
                _dbContext.SaveChanges();
            }

            // Now delete the brand
            _dbContext.Brand.Remove(brandInDb);
            _dbContext.SaveChanges();

            return Json(new { success = true, message = "Brand deleted successfully." });
        }

        [HttpPost]
        public ActionResult BrandDetails(ProductViewModel model)
        {
            ModelState.Remove("Brands.Id");
            if (ModelState.IsValid)
            {
                if (model.Brands.Id != 0)
                {
                    var brandInDb = _dbContext.Brand.SingleOrDefault(c => c.Id == model.Brands.Id);
                    brandInDb.Name = model.Brands.Name;
                    _dbContext.SaveChanges();
                    return Json(new { success = true, message = "Brand details updated successfully." });
                }
                else
                {
                    _dbContext.Brand.Add(model.Brands);
                    _dbContext.SaveChanges();
                    return Json(new { success = true, message = "Brand details added successfully." });
                }
            }
            return View(model);
        }
    }
}
