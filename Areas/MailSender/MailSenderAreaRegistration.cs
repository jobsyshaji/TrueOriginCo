using System.Web.Mvc;

namespace CherukarasThejas.Areas.MailSender
{
    public class MailSenderAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "MailSender";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "MailSender_default",
                "MailSender/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}