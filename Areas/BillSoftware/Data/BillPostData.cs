using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CherukarasThejas.Areas.BillSoftware.Data
{
    //public class BillPostData
    //{
    //    public List<ProductList> ProductList { get; set; }
    //    public string Name { get; set; }
    //    public string MobileNumber { get; set; }
    //    public string Invoiceno { get; set; }
    //    public int BId { get; set; }
    //    public int CustomerId { get; set; }
    //    public DateTime BillDate { get; set; }
    //    public string GSTNumber { get; set; } // Customer's GSTIN

    //    // --- New/Updated Properties for Totals ---
    //    public decimal TotalTaxable { get; set; }
    //    public decimal TotalCGST { get; set; }
    //    public decimal TotalSGST { get; set; }
    //    public decimal TotalIGST { get; set; }
    //    public decimal GrandTotal { get; set; } // Renamed from TotalPrice for clarity

    //    // These seem more related to payment tracking, which is fine
    //    public string PaymentMode { get; set; }
    //    public string PaymentStatus { get; set; }
    //    public decimal PaidAmount { get; set; }
    //    public decimal RemainingBalance { get; set; }
    //    public DateTime? PaymentDate { get; set; } // Nullable DateTime is better

    //}

    public class BillPostData
    {
        // --- Customer & Invoice Details ---
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string GSTNumber { get; set; }
        public string Invoiceno { get; set; }
        public DateTime BillDate { get; set; }

        /// <summary>
        /// A list of all products included in this bill.
        /// </summary>
        public List<ProductList> ProductList { get; set; }

        // --- Grand Totals & Summary ---
        public decimal TotalTaxable { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal GrandTotal { get; set; }

        // --- Optional Database/Tracking Fields ---
        public int BId { get; set; }
        public int CustomerId { get; set; }

        // --- Optional Payment Tracking Fields ---
        public string PaymentMode { get; set; }
        public string PaymentStatus { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}