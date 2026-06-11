using CherukarasThejas.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CherukarasThejas.Models.DatabaseContext
{
    public class AppDbContext: DbContext
    {
        public AppDbContext() :base("cherukaraConn") { }
        public DbSet<ProductMaster> ProductMasters { get; set; } 
    }
}