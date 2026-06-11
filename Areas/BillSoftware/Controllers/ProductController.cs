using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace CherukarasThejas.Areas.BillSoftware.Controllers
{
    [RouteArea("BillSoftware")]
    public class ProductController : Controller
    {

        public ActionResult AddProduct()
        {

            return View();
        }


    
    }
}