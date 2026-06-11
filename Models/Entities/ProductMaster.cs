using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CherukarasThejas.Models.Entities
{
    [Table("productmaster")]
    public class ProductMaster
    {
        /// <summary>
        /// Maps to the PId (Primary Key, Auto-Increment) column.
        /// C# type: int
        /// </summary>
        [Key]
        public int PId { get; set; }

        /// <summary>
        /// Maps to the InDate column.
        /// C# type: DateTime
        /// </summary>
        public DateTime InDate { get; set; }

        /// <summary>
        /// Maps to the PTitle column.
        /// C# type: string
        /// </summary>
        public string PTitle { get; set; }

        /// <summary>
        /// Maps to the PQuantity column.
        /// C# type: decimal (for precision in quantities)
        /// </summary>
        public decimal PQuantity { get; set; }

        /// <summary>
        /// Maps to the AvailableStock column.
        /// C# type: decimal
        /// </summary>
        public decimal AvailableStock { get; set; }

        /// <summary>
        /// Maps to the FinalRate column. 
        /// C# type: decimal (best for currency)
        /// </summary>
        public decimal FinalRate { get; set; }

        /// <summary>
        /// Maps to the PNameEN (English Name) column.
        /// C# type: string
        /// </summary>
        public string PNameEN { get; set; }

        /// <summary>
        /// Maps to the PNameML (Malayalam Name) column.
        /// C# type: string
        /// </summary>
        public string PNameML { get; set; }

        /// <summary>
        /// Maps to the HSNNumber column. As a string, it can be null.
        /// C# type: string
        /// </summary>
        public string HSNNumber { get; set; }

        /// <summary>
        /// Maps to the CGSTPercentage column.
        /// C# type: decimal
        /// </summary>
        public decimal CGSTPercentage { get; set; }

        /// <summary>
        /// Maps to the SGSTPercentage column.
        /// C# type: decimal
        /// </summary>
        public decimal SGSTPercentage { get; set; }

        /// <summary>
        /// Maps to the IGSTPercentage column.
        /// C# type: decimal
        /// </summary>
        public decimal IGSTPercentage { get; set; }
    }
}