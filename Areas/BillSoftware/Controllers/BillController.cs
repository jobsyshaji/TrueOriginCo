using BillManagementSoftware.Repository.DbConfig;
using CherukarasThejas.Areas.BillSoftware.Data;
using CherukarasThejas.Models.Entities;
using ExcelDataReader;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CherukarasThejas.Areas.BillSoftware.Controllers
{
    public class BillController : Controller
    {
        private readonly IMysqlData _mysqlData;
        public BillController(IMysqlData mysqlData)
        {
            _mysqlData = mysqlData;
        }

        public ActionResult GenerateBill()
        {
            ViewBag.InvoiceNumber = GenerateInvoiceNumber();
            return View();
        }

        // Renamed from AllBills to match previous implementation
        public ActionResult ListBills()
        {
            List<BillListViewModel> bills = new List<BillListViewModel>();
            try
            {
                using (var connection = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    connection.Open();
                    // This query fetches the main details for all bills, showing the newest ones first.
                    string query = "SELECT BillId, InvoiceNo, BillDate, CustomerName, GrandTotal FROM Bills ORDER BY BillId DESC";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bills.Add(new BillListViewModel
                                {
                                    BillId = Convert.ToInt32(reader["BillId"]),
                                    InvoiceNo = reader["InvoiceNo"].ToString(),
                                    BillDate = Convert.ToDateTime(reader["BillDate"]),
                                    CustomerName = reader["CustomerName"].ToString(),
                                    GrandTotal = Convert.ToDecimal(reader["GrandTotal"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and show an error message on the page
                ViewBag.ErrorMessage = "An error occurred while retrieving bills from the database.";
                // System.Diagnostics.Debug.WriteLine(ex); // For debugging
            }

            // Pass the list of bills to the view
            return View(bills);
        }

        [HttpPost]
        public ActionResult BillTemplate(BillPostData data)
        {
            return PartialView("_BillTemplate", data);
        }

        [HttpPost]
        public ActionResult BulkUpload(HttpPostedFileBase excelFile, string customerName, string customerMobile, string customerGstin)
        {
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerMobile))
            {
                ViewBag.Message = "Customer Name and Mobile Number are required.";
                return View();
            }
            if (excelFile == null || excelFile.ContentLength == 0)
            {
                ViewBag.Message = "Please select a file to upload.";
                return View();
            }
            if (!excelFile.FileName.EndsWith(".xlsx"))
            {
                ViewBag.Message = "Invalid file format. Please upload an .xlsx file.";
                return View();
            }

            var invoicesToCreate = new List<BillPostData>();
            int successCount = 0;
            try
            {
                using (var stream = excelFile.InputStream)
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        DataTable dataTable = result.Tables[0];
                        var excelData = new List<ExcelRowDTO>();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            try
                            {
                                object dateValue = row["BillDate"];
                                DateTime billDateValue;
                                if (dateValue is DateTimeOffset dto) { billDateValue = dto.DateTime; }
                                else { billDateValue = Convert.ToDateTime(dateValue); }

                                excelData.Add(new ExcelRowDTO
                                {
                                    InvoiceGroupID = row["InvoiceGroupID"]?.ToString().Trim(),
                                    BillDate = billDateValue,
                                    ProductID = Convert.ToInt32(row["ProductID"]),
                                    Quantity = Convert.ToDecimal(row["Quantity"])
                                });
                            }
                            catch (Exception ex)
                            {
                                string groupID = row["InvoiceGroupID"]?.ToString() ?? "N/A";
                                throw new Exception($"Error processing row for InvoiceGroupID '{groupID}'. Please check the data format. Details: {ex.Message}");
                            }
                        }

                        var groupedByInvoice = excelData.Where(r => !string.IsNullOrEmpty(r.InvoiceGroupID))
                                                        .GroupBy(r => r.InvoiceGroupID)
                                                        .OrderBy(g => g.First().BillDate);

                        foreach (var invoiceGroup in groupedByInvoice)
                        {
                            var firstRow = invoiceGroup.First();
                            var bill = new BillPostData
                            {
                                Invoiceno = GenerateInvoiceNumber(),
                                Name = customerName,
                                MobileNumber = customerMobile,
                                BillDate = firstRow.BillDate,
                                GSTNumber = customerGstin,
                                ProductList = new List<ProductList>()
                            };

                            foreach (var itemRow in invoiceGroup)
                            {
                                ProductMaster productDetails = GetProductFromDb(itemRow.ProductID);
                                if (productDetails == null)
                                {
                                    throw new Exception($"Product with ID '{itemRow.ProductID}' not found.");
                                }

                                // <-- FIX: Correct calculation logic. First get the base rate, then calculate taxable amount.
                                decimal baseRate = CalculateBaseRateFromFinalPrice(productDetails.FinalRate, productDetails.CGSTPercentage, productDetails.SGSTPercentage);
                                var taxableAmount = itemRow.Quantity * baseRate;
                                var cgstAmount = taxableAmount * (productDetails.CGSTPercentage / 100);
                                var sgstAmount = taxableAmount * (productDetails.SGSTPercentage / 100);
                                var igstAmount = taxableAmount * (productDetails.IGSTPercentage / 100);

                                bill.ProductList.Add(new ProductList
                                {
                                    Id = itemRow.ProductID,
                                    ProductName = productDetails.PNameEN,
                                    HSN = productDetails.HSNNumber,
                                    Quantity = itemRow.Quantity,
                                    Rate = baseRate, // <-- FIX: Use the calculated base rate
                                    TaxableAmount = taxableAmount,
                                    CGSTPercentage = productDetails.CGSTPercentage,
                                    SGSTPercentage = productDetails.SGSTPercentage,
                                    IGSTPercentage = productDetails.IGSTPercentage,
                                    CGSTAmount = cgstAmount,
                                    SGSTAmount = sgstAmount,
                                    IGSTAmount = igstAmount,
                                    TotalAmount = taxableAmount + cgstAmount + sgstAmount + igstAmount
                                });
                            }

                            bill.TotalTaxable = bill.ProductList.Sum(p => p.TaxableAmount);
                            bill.TotalCGST = bill.ProductList.Sum(p => p.CGSTAmount);
                            bill.TotalSGST = bill.ProductList.Sum(p => p.SGSTAmount);
                            bill.TotalIGST = bill.ProductList.Sum(p => p.IGSTAmount);
                            bill.GrandTotal = bill.ProductList.Sum(p => p.TotalAmount);

                            invoicesToCreate.Add(bill);

                            bool isSuccess = SaveBillToDatabase(bill);
                            if (isSuccess)
                            {
                                successCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "An error occurred while processing the file: " + ex.Message;
                return View();
            }

            ViewBag.Message = $"{successCount} out of {invoicesToCreate.Count} invoices were generated and saved successfully!";
            return View();
        }

        public ActionResult PrintSavedBill(int billId)
        {
            BillPostData billData = null;
            try
            {
                using (var connection = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    connection.Open();

                    // --- Step 1: Get the main bill details from the 'Bills' table ---
                    string billQuery = "SELECT * FROM Bills WHERE BillId = @BillId";
                    using (var cmdBill = new MySqlCommand(billQuery, connection))
                    {
                        cmdBill.Parameters.AddWithValue("@BillId", billId);
                        using (var reader = cmdBill.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate a BillPostData object with the header info
                                billData = new BillPostData
                                {
                                    BId = Convert.ToInt32(reader["BillId"]),
                                    Invoiceno = reader["InvoiceNo"].ToString(),
                                    BillDate = Convert.ToDateTime(reader["BillDate"]),
                                    Name = reader["CustomerName"].ToString(),
                                    MobileNumber = reader["CustomerMobile"].ToString(),
                                    GSTNumber = reader["CustomerGSTIN"].ToString(),
                                    TotalTaxable = Convert.ToDecimal(reader["TotalTaxable"]),
                                    TotalCGST = Convert.ToDecimal(reader["TotalCGST"]),
                                    TotalSGST = Convert.ToDecimal(reader["TotalSGST"]),
                                    TotalIGST = Convert.ToDecimal(reader["TotalIGST"]),
                                    GrandTotal = Convert.ToDecimal(reader["GrandTotal"]),
                                    ProductList = new List<ProductList>() // Initialize the list for the items
                                };
                            }
                        } // The reader is closed here
                    }

                    // If no bill was found with that ID, return a "Not Found" error
                    if (billData == null)
                    {
                        return HttpNotFound($"No invoice found with ID {billId}.");
                    }

                    // --- Step 2: Get all the line items for this bill from the 'BillItems' table ---
                    string itemsQuery = "SELECT * FROM BillItems WHERE BillId = @BillId";
                    using (var cmdItems = new MySqlCommand(itemsQuery, connection))
                    {
                        cmdItems.Parameters.AddWithValue("@BillId", billId);
                        using (var reader = cmdItems.ExecuteReader())
                        {
                            // Loop through all the items and add them to the ProductList
                            while (reader.Read())
                            {
                                billData.ProductList.Add(new ProductList
                                {
                                    Id = Convert.ToInt32(reader["ProductId"]),
                                    ProductName = reader["ProductName"].ToString(),
                                    HSN = reader["HSN"].ToString(),
                                    Quantity = Convert.ToDecimal(reader["Quantity"]),
                                    Rate = Convert.ToDecimal(reader["Rate"]),
                                    TaxableAmount = Convert.ToDecimal(reader["TaxableAmount"]),
                                    CGSTPercentage = Convert.ToDecimal(reader["CGSTPercentage"]),
                                    CGSTAmount = Convert.ToDecimal(reader["CGSTAmount"]),
                                    SGSTPercentage = Convert.ToDecimal(reader["SGSTPercentage"]),
                                    SGSTAmount = Convert.ToDecimal(reader["SGSTAmount"]),
                                    IGSTPercentage = Convert.ToDecimal(reader["IGSTPercentage"]),
                                    IGSTAmount = Convert.ToDecimal(reader["IGSTAmount"]),
                                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"])
                                });
                            }
                        }
                    }
                } // The connection is closed here
            }
            catch (Exception ex)
            {
                // Log the exception and return a user-friendly error message
                // System.Diagnostics.Debug.WriteLine(ex);
                return Content("An error occurred while retrieving the bill details: " + ex.Message);
            }

            // --- Step 3: Return our reusable print template with the complete, reconstructed data ---
            return View("_BillTemplate", billData);
        }

        // --- PRIVATE HELPER METHODS ---

        private string GenerateInvoiceNumber()
        {
            // This method seems correct, no changes needed.
            string prefix = "BC";
            int currentYear = DateTime.Now.Year;
            string yearPrefix = $"{prefix}/{currentYear}/";
            int nextNumber = 1;
            try
            {
                using (var connection = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    connection.Open();
                    string query = "SELECT InvoiceNo FROM Bills WHERE InvoiceNo LIKE @YearPrefix ORDER BY BillId DESC LIMIT 1";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@YearPrefix", yearPrefix + "%");
                        var lastInvoiceNumber = cmd.ExecuteScalar() as string;
                        if (!string.IsNullOrEmpty(lastInvoiceNumber))
                        {
                            string numericPart = lastInvoiceNumber.Substring(yearPrefix.Length);
                            if (int.TryParse(numericPart, out int lastNumber))
                            {
                                nextNumber = lastNumber + 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return $"ERROR-GEN-{DateTime.Now.Ticks}";
            }
            string formattedNumber = nextNumber.ToString("D3");
            return $"{yearPrefix}{formattedNumber}";
        }

        private ProductMaster GetProductFromDb(int pId)
        {
            ProductMaster product = null;
            try
            {
                using (var connection = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    connection.Open();
                    string query = "SELECT * FROM productmaster WHERE PId = @ProductId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", pId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                product = new ProductMaster
                                {
                                    PId = Convert.ToInt32(reader["PId"]),
                                    InDate = Convert.ToDateTime(reader["InDate"]),
                                    PTitle = reader["PTitle"].ToString(),
                                    PQuantity = Convert.ToDecimal(reader["PQuantity"]),
                                    AvailableStock = Convert.ToDecimal(reader["AvailableStock"]),
                                    FinalRate = Convert.ToDecimal(reader["FinalRate"]),
                                    PNameEN = reader["PNameEN"].ToString(),
                                    PNameML = reader["PNameML"].ToString(),
                                    HSNNumber = reader.IsDBNull(reader.GetOrdinal("HSNNumber")) ? null : reader["HSNNumber"].ToString(),
                                    CGSTPercentage = Convert.ToDecimal(reader["CGSTPercentage"]),
                                    SGSTPercentage = Convert.ToDecimal(reader["SGSTPercentage"]),
                                    IGSTPercentage = Convert.ToDecimal(reader["IGSTPercentage"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetProductFromDb: " + ex.ToString());
            }
            return product;
        }

        private bool SaveBillToDatabase(BillPostData data)
        {
            MySqlConnection connection = null;
            MySqlTransaction transaction = null;
            try
            {
                connection = new MySqlConnection(_mysqlData.GetConnectionString());
                connection.Open();
                transaction = connection.BeginTransaction();
                string billInsertQuery = @"
                    INSERT INTO Bills (InvoiceNo, BillDate, CustomerName, CustomerMobile, CustomerGSTIN, 
                                       TotalTaxable, TotalCGST, TotalSGST, TotalIGST, GrandTotal)
                    VALUES (@Invoiceno, @BillDate, @Name, @MobileNumber, @GSTNumber, 
                            @TotalTaxable, @TotalCGST, @TotalSGST, @TotalIGST, @GrandTotal);
                    SELECT LAST_INSERT_ID();";

                int newBillId;
                using (var cmdBill = new MySqlCommand(billInsertQuery, connection, transaction))
                {
                    cmdBill.Parameters.AddWithValue("@Invoiceno", data.Invoiceno);
                    cmdBill.Parameters.AddWithValue("@BillDate", data.BillDate);
                    cmdBill.Parameters.AddWithValue("@Name", data.Name);
                    cmdBill.Parameters.AddWithValue("@MobileNumber", data.MobileNumber);
                    cmdBill.Parameters.AddWithValue("@GSTNumber", data.GSTNumber);
                    cmdBill.Parameters.AddWithValue("@TotalTaxable", data.TotalTaxable);
                    cmdBill.Parameters.AddWithValue("@TotalCGST", data.TotalCGST);
                    cmdBill.Parameters.AddWithValue("@TotalSGST", data.TotalSGST);
                    cmdBill.Parameters.AddWithValue("@TotalIGST", data.TotalIGST);
                    cmdBill.Parameters.AddWithValue("@GrandTotal", data.GrandTotal);
                    var result = cmdBill.ExecuteScalar();
                    newBillId = Convert.ToInt32(result);
                }

                foreach (var item in data.ProductList)
                {
                    // <-- FIX: Changed column from FinalRate to Rate to match the BillItems table design
                    string itemInsertQuery = @"
                        INSERT INTO BillItems (BillId, ProductId, ProductName, HSN, Quantity, Rate, TaxableAmount,
                                               CGSTPercentage, CGSTAmount, SGSTPercentage, SGSTAmount, 
                                               IGSTPercentage, IGSTAmount, TotalAmount)
                        VALUES (@BillId, @ProductId, @ProductName, @HSN, @Quantity, @Rate, @TaxableAmount,
                                @CGSTPercentage, @CGSTAmount, @SGSTPercentage, @SGSTAmount,
                                @IGSTPercentage, @IGSTAmount, @TotalAmount);";

                    using (var cmdItem = new MySqlCommand(itemInsertQuery, connection, transaction))
                    {
                        cmdItem.Parameters.AddWithValue("@BillId", newBillId);
                        cmdItem.Parameters.AddWithValue("@ProductId", item.Id);
                        cmdItem.Parameters.AddWithValue("@ProductName", item.ProductName);
                        cmdItem.Parameters.AddWithValue("@HSN", item.HSN);
                        cmdItem.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmdItem.Parameters.AddWithValue("@Rate", item.Rate); // <-- FIX: Use item.Rate (the base rate)
                        cmdItem.Parameters.AddWithValue("@TaxableAmount", item.TaxableAmount);
                        cmdItem.Parameters.AddWithValue("@CGSTPercentage", item.CGSTPercentage);
                        cmdItem.Parameters.AddWithValue("@CGSTAmount", item.CGSTAmount);
                        cmdItem.Parameters.AddWithValue("@SGSTPercentage", item.SGSTPercentage);
                        cmdItem.Parameters.AddWithValue("@SGSTAmount", item.SGSTAmount);
                        cmdItem.Parameters.AddWithValue("@IGSTPercentage", item.IGSTPercentage);
                        cmdItem.Parameters.AddWithValue("@IGSTAmount", item.IGSTAmount);
                        cmdItem.Parameters.AddWithValue("@TotalAmount", item.TotalAmount);
                        cmdItem.ExecuteNonQuery();
                    }

                    string stockUpdateQuery = @"
                        UPDATE productmaster 
                        SET AvailableStock = AvailableStock - @Quantity 
                        WHERE PId = @ProductId;";

                    using (var cmdStock = new MySqlCommand(stockUpdateQuery, connection, transaction))
                    {
                        cmdStock.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmdStock.Parameters.AddWithValue("@ProductId", item.Id);
                        cmdStock.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                try { transaction?.Rollback(); } catch { }
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
            finally
            {
                if (connection?.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private decimal CalculateBaseRateFromFinalPrice(decimal finalPrice, decimal cgstPercentage, decimal sgstPercentage)
        {
            decimal totalGstDecimal = (cgstPercentage + sgstPercentage) / 100m;
            decimal divisor = 1 + totalGstDecimal;
            if (divisor == 0) return 0;
            decimal baseRate = finalPrice / divisor;
            return Math.Round(baseRate, 2);
        }

        private class ExcelRowDTO
        {
            public string InvoiceGroupID { get; set; }
            public DateTime BillDate { get; set; }
            public int ProductID { get; set; }
            public decimal Quantity { get; set; }
        }
    }

}