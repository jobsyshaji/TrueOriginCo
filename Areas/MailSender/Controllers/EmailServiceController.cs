using CherukarasThejas.Areas.MailSender.Data;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CherukarasThejas.Areas.MailSender.Controllers
{
    public class EmailServiceController : Controller
    {
        // GET: MailSender/EmailService
        public ActionResult SendAutomatedMail()  
        {
            return View();  
        }

        [HttpPost]
        public async Task<string> SendEmailAsync(EmailRequest request)
        {
            try
            {
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "template.txt");
               
                var bodyTemplate = System.IO.File.ReadAllText(templatePath);
                var body = bodyTemplate
                 .Replace("{hrname}", request.HrName)
                 .Replace("{companyname}", request.CompanyName)
                 .Replace("{jobprofile}", request.JobProfile)
                 .Replace("\n", "<br>")
                 .Replace("\r", "");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Job Cherukara Shaji", "csjob.developer@gmail.com"));
                message.To.Add(new MailboxAddress("", request.ToEmail));
                //message.Subject = $"Application for {request.JobProfile} Role at {request.CompanyName}";
                message.Subject = $"Application for {request.JobProfile} – Available Immediately | 4 YOE in .NET Development";

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };

                string resumePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "CS_Job_Resume_SoftwareEngineer.pdf");

                builder.Attachments.Add(resumePath);
                message.Body = builder.ToMessageBody();

                using( var client = new SmtpClient()){
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("csjob.developer@gmail.com", "nylokazbhmdciabr");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                }

                return $"SUCCESS: Email sent to {request.ToEmail} for {request.JobProfile} at {request.CompanyName}";
            }
            catch (Exception ex)
            {
                return $"FAILURE: Error sending email to {request.ToEmail}. Error: {ex.Message}";                
            }
        }

        //public async Task<EmailRequest> GetMailInfo(string email)
        //{
        //    EmailRequest response = new EmailRequest();
        //    try
        //    {

        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }
}