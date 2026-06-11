using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CherukarasThejas.Areas.BillSoftware.Data
{
    public class BillListViewModel
    {
        public int BillId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public decimal GrandTotal { get; set; }
    }
}