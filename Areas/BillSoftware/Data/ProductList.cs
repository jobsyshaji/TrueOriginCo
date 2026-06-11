using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CherukarasThejas.Areas.BillSoftware.Data
{
    //public class ProductList
    //{
    //    public int Id { get; set; } // Product ID
    //    public string ProductName { get; set; }
    //    public string HSN { get; set; }
    //    public decimal Quantity { get; set; }
    //    public decimal Rate { get; set; }
    //    public decimal TaxableAmount { get; set; }
    //    public decimal CGSTPercentage { get; set; }
    //    public decimal CGSTAmount { get; set; }
    //    public decimal SGSTPercentage { get; set; }
    //    public decimal SGSTAmount { get; set; }
    //    public decimal IGSTPercentage { get; set; }
    //    public decimal IGSTAmount { get; set; }
    //    public decimal TotalAmount { get; set; }
    //}

    public class ProductList
    {
        public int Id { get; set; } // The Product's ID from the database (PId)
        public string ProductName { get; set; }
        public string HSN { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; } 
        public decimal TaxableAmount { get; set; }
        public decimal CGSTPercentage { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTPercentage { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTPercentage { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}