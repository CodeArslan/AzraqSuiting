using AzraqSuiting.Models;
using AzraqSuiting.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzraqSuiting.Controllers
{
    public class CategoryController : Controller
    {
        private ApplicationDbContext _dbContext;
        public CategoryController()
        {
            _dbContext = new ApplicationDbContext();
        }

        // GET: Category
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CategoryDetails(ProductViewModel model)
        {
            ModelState.Remove("Categories.Id");
            if (ModelState.IsValid)
            {
                if (model.Categories.Id != 0)
                {
                    var categoryInDb = _dbContext.Category.SingleOrDefault(c => c.Id == model.Categories.Id);
                    categoryInDb.Name = model.Categories.Name;
                    _dbContext.SaveChanges();
                    return Json(new { success = true, message = "Category details updated successfully." });
                }
                else
                {
                    _dbContext.Category.Add(model.Categories);
                    _dbContext.SaveChanges();
                    return Json(new { success = true, message = "Category details added successfully." });
                }
            }
            return View(model);
        }
    }
}