using System.Web.Mvc;

namespace CherukarasThejas.Areas.BillSoftware
{
    public class BillSoftwareAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "BillSoftware";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "BillSoftware_default",
                "BillSoftware/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}