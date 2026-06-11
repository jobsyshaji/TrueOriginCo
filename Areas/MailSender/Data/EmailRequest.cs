using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CherukarasThejas.Areas.MailSender.Data
{
    public class EmailRequest
    {
        public string ToEmail { get; set; }
        public string CompanyName { get; set; }
        public string HrName { get; set; }
        public string JobProfile { get; set; }
    }
}