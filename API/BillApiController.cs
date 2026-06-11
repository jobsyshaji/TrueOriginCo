using BillManagementSoftware.Repository.DbConfig;
using BillManagementSoftware.Repository.Interface;
using CherukarasThejas.Areas.BillSoftware.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq; // <-- NOTE: Added for .Sum()

namespace BillManagementSoftware.Controllers.API
{
    public class BillApiController : ApiController
    {
        private readonly IBillMethod _billMethod;
        private readonly IMysqlData _mysqlData;
        public BillApiController(IBillMethod billMethod, IMysqlData mysqlData)
        {
            _billMethod = billMethod;
            _mysqlData = mysqlData;
        }

        [HttpGet]
        [Route("get-customername/{mobile}", Name = "B-GetCustomerName")]
        public async Task<IHttpActionResult> GetCustomerName(string mobile)
        {
            return Ok(await _billMethod.GetCustomerName(mobile));
        }

        [HttpGet]
        [Route("All-Bills", Name = "A-List")] // <-- FIX: Renamed route for clarity
        public IHttpActionResult AllBills() // <-- FIX: Renamed method for clarity
        {
            var list = new List<BillListViewModel>(); // <-- FIX: Using the lightweight ViewModel
            try
            {
                using (MySqlConnection mc = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    mc.Open();
                    // <-- FIX: Querying the correct 'Bills' table with the correct columns.
                    string query = @"SELECT BillId, InvoiceNo, BillDate, CustomerName, GrandTotal 
                                     FROM Bills 
                                     ORDER BY BillId DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, mc))
                    {
                        var reader = cmd.ExecuteReader();
                        // <-- FIX: Implemented the read logic.
                        while (reader.Read())
                        {
                            list.Add(new BillListViewModel
                            {
                                BillId = Convert.ToInt32(reader["BillId"]),
                                InvoiceNo = reader["InvoiceNo"].ToString(),
                                BillDate = Convert.ToDateTime(reader["BillDate"]),
                                CustomerName = reader["CustomerName"].ToString(),
                                GrandTotal = Convert.ToDecimal(reader["GrandTotal"])
                            });
                        }
                    } // Reader is closed here by 'using'
                }
            }
            catch (Exception ex)
            {
                // It's better to return an error than to throw, which can crash the app pool.
                return InternalServerError(ex);
            }
            return Ok(list);
        }

        [HttpGet]
        [Route("ViewInvoice/{billId}", Name = "ViewInvoice")]
        public IHttpActionResult ViewInvoice(int billId)
        {
            // <-- FIX: This method now correctly fetches the full bill details and returns a BillPostData object.
            BillPostData billData = null;
            try
            {
                using (MySqlConnection mc = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    mc.Open();
                    // Step 1: Get the main bill details from the 'Bills' table
                    string billQuery = "SELECT * FROM Bills WHERE BillId = @BillId";
                    using (var cmdBill = new MySqlCommand(billQuery, mc))
                    {
                        cmdBill.Parameters.AddWithValue("@BillId", billId);
                        using (var reader = cmdBill.ExecuteReader())
                        {
                            if (reader.Read())
                            {
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
                                    ProductList = new List<ProductList>() // Initialize the list
                                };
                            }
                        }
                    }

                    if (billData == null)
                    {
                        return NotFound(); // No bill found with that ID
                    }

                    // Step 2: Get all line items for this bill from the 'BillItems' table
                    string itemsQuery = "SELECT * FROM BillItems WHERE BillId = @BillId";
                    using (var cmdItems = new MySqlCommand(itemsQuery, mc))
                    {
                        cmdItems.Parameters.AddWithValue("@BillId", billId);
                        using (var reader = cmdItems.ExecuteReader())
                        {
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
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(billData);
        }

        [HttpPost]
        [Route("api/bills/add", Name = "B-Add")]
        public IHttpActionResult AddBill([FromBody] BillPostData data)
        {
            if (data == null || data.ProductList == null || !data.ProductList.Any())
            {
                return BadRequest("Invoice data is incomplete.");
            }

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
                    // <-- FIX: Changed column from FinalRate to Rate to match the BillItems table design.
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
                        cmdItem.Parameters.AddWithValue("@Rate", item.Rate); // <-- FIX: Using item.Rate (the calculated base rate)
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
            }
            catch (Exception ex)
            {
                try { transaction?.Rollback(); } catch { }
                return InternalServerError(ex);
            }
            finally
            {
                if (connection?.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return Ok(new { Status = true, Message = "Bill saved successfully!" });
        }
    }
}