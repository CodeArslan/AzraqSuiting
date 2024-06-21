using AzraqSuiting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzraqSuiting.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Customer
        private ApplicationDbContext _dbContext;
        public CustomerController()
        {
            _dbContext = new ApplicationDbContext();
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetCustomerDetailsForDropdown(string q)
        {
            var customer = _dbContext.Customer
                .Where(p => p.Name.Contains(q))
                .Select(p => new {
                    id = p.Id,
                    name = p.Name,
                    phoneNumber = p.PhoneNumber
                })
                .ToList();

            return Json(new { results = customer }, JsonRequestBehavior.AllowGet);
        }
    }
}