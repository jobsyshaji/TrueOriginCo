using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CherukarasThejas.Areas.BillSoftware.Data
{
    public class ProductPost
    {

        public int? PId { get; set; }

        [Required]
        public string PName { get; set; }

        public string Rate { get; set; }

        public string AvailableStock { get; set; }

        public DateTime Indate { get; set; }

        public string PQuantity { get; set; }

        public string Description { get; set; }
    }
}