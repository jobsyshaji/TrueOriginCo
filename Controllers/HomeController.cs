using CherukarasThejas.Models.DatabaseContext;
using CherukarasThejas.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CherukarasThejas.Controllers
{
    [RoutePrefix("~/")]
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> ProductListWithPrice() 
        {
            List<ProductMaster> list = await GetProductListWithPrice();
            return View(list);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<List<ProductMaster>> GetProductListWithPrice()
        {
            List<ProductMaster> list = new List<ProductMaster>();
            try
            {
                using (AppDbContext _db = new AppDbContext())
                {
                    list = await _db.ProductMasters.ToListAsync();
                }
            }
            catch (Exception ex)
            {
            }
            return list;
        }
    }
}