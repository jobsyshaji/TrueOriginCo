using BillManagementSoftware.Repository.DbConfig;
using BillManagementSoftware.Repository.Interface;
using BillManagementSoftware.Repository.Service;
using CherukarasThejas.Areas.BillSoftware.Data;
using CherukarasThejas.Models.Entities;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;

namespace BillManagementSoftware.Controllers.API
{

    [RoutePrefix("api/Product")]
    public class ProductApiController : ApiController
    {

        private readonly IProductMethod _productMethod;
        private readonly IMysqlData _mysqlData;
        public ProductApiController(ProductMethod productMethod, IMysqlData mysqlData)
        {
            _productMethod = productMethod;
            _mysqlData = mysqlData;
        }

        [Route("save-product", Name = "P-Save")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveProduct(ProductPost pd)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _productMethod.SaveProduct(pd));
            }
            return BadRequest("Invalid Request Body");
        }

        [HttpGet]
        [Route("list", Name = "P-List")]
        public IHttpActionResult GetProductList()
        {
            List<ProductPost> customer = new List<ProductPost>();
            try
            {

                using (MySqlConnection cou = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    cou.Open();
                    string query = "select PId,PNameEN,Rate,AvailableStock,PQuantity,Indate from productmaster";
                    using (MySqlCommand cmd = new MySqlCommand(query, cou))
                    {
                        var Reader = cmd.ExecuteReader();//to read all data
                        ///ExecuteNonQuery() to perform insert,update,delete which returns no response
                        //ExecuteScalar() is used to display data of only one row
                        while (Reader.Read())
                        {
                            customer.Add(new ProductPost
                            {
                                PId = Convert.ToInt32(Reader["PId"]),
                                PName = Reader["PNameEN"].ToString(),
                                Rate = Reader["Rate"].ToString(),
                                AvailableStock = Reader["AvailableStock"].ToString(),
                                PQuantity = Reader["PQuantity"].ToString(),
                                Indate = Convert.ToDateTime(Reader["Indate"])

                            });
                        }
                        Reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return Ok(customer);
        }

        [HttpPost]
        [Route("delete/{pid}", Name = "P-Delete")]
        public async Task<IHttpActionResult> DeleteProduct(int pid)
        {
            return Ok(await _productMethod.DeleteProduct(pid));
        }

        [HttpGet]
        [Route("edit/{pid}", Name = "P-Edit")]
        public IHttpActionResult EditProduct(int pid)
        {
            ProductPost product = new ProductPost();
            try
            {
                using (MySqlConnection cou = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    cou.Open();
                    string query = "select PId,PNameEN,Rate,AvailableStock,PQuantity,Indate from productmaster where pid= @pid";
                    using (MySqlCommand cmd = new MySqlCommand(query, cou))
                    {
                        cmd.Parameters.AddWithValue("@pid", pid);
                        var Reader = cmd.ExecuteReader();//to read all data
                        ///ExecuteNonQuery() to perform insert,update,delete which returns no response
                        //ExecuteScalar() is used to display data of only one row
                        while (Reader.Read())
                        {
                            product.PId = Convert.ToInt32(Reader["PId"]);
                            product.PName = Reader["PNameEN"].ToString();
                            product.Rate = Reader["Rate"].ToString();
                            product.AvailableStock = Reader["AvailableStock"].ToString();
                            product.PQuantity = Reader["PQuantity"].ToString();
                            product.Indate = Convert.ToDateTime(Reader["Indate"]);
                        }
                        Reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return Ok(product);
        }

        [HttpGet]
        [Route("drp-list", Name = "P-DrpList")]
        public async Task<IHttpActionResult> GetProductDrpList()
        {
            return Ok(await _productMethod.GetProductDrpList());
        }

        [HttpGet]
        [Route("product-rate/{pId}", Name = "P-Rate")]
        public async Task<IHttpActionResult> GetProductRate(int pId)
        {
            return Ok(await _productMethod.GetProductRate(pId));
        }

        [HttpGet]
        [Route("product-details/{pId}", Name = "P-Details")]
        public IHttpActionResult GetProductDetails(int pId)
        {
            ProductMaster product = null;

            try
            {
                using (MySqlConnection con = new MySqlConnection(_mysqlData.GetConnectionString()))
                {
                    con.Open();

                    // 1. --- UPDATED QUERY ---
                    // Renamed 'Rate' to 'FinalRate' to match your database table
                    string query = @"SELECT 
                                    PId, PNameEN, FinalRate, AvailableStock, PQuantity, InDate,
                                    HSNNumber, CGSTPercentage, SGSTPercentage, IGSTPercentage 
                                 FROM productmaster 
                                 WHERE PId = @ProductId";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", pId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                product = new ProductMaster
                                {
                                    PId = Convert.ToInt32(reader["PId"]),
                                    PNameEN = reader["PNameEN"].ToString(),

                                    // 2. --- UPDATED MAPPING ---
                                    // Map the 'FinalRate' column from the database to our entity
                                    FinalRate = Convert.ToDecimal(reader["FinalRate"]),

                                    AvailableStock = Convert.ToDecimal(reader["AvailableStock"]),
                                    PQuantity = Convert.ToDecimal(reader["PQuantity"]),
                                    InDate = Convert.ToDateTime(reader["InDate"]),
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
                // Log the exception ex here for debugging
                return InternalServerError(ex);
            }

            if (product == null)
            {
                return NotFound();
            }

            // 3. --- NEW CALCULATION LOGIC ---
            // Calculate the base rate from the stored final rate
            decimal calculatedBaseRate = CalculateBaseRateFromFinalPrice(
                product.FinalRate,
                product.CGSTPercentage,
                product.SGSTPercentage
            );

            // 4. --- UPDATED RETURN OBJECT ---
            // Return a new object that includes the CALCULATED base rate in a property
            // named 'Rate', because this is what the front-end JavaScript expects.
            return Ok(new
            {
                Rate = calculatedBaseRate,
                product.HSNNumber,
                product.CGSTPercentage,
                product.SGSTPercentage,
                product.IGSTPercentage
            });
        }

        /// <summary>
        /// Calculates the pre-tax base rate from a final price that includes GST.
        /// </summary>
        private decimal CalculateBaseRateFromFinalPrice(decimal finalPrice, decimal cgstPercentage, decimal sgstPercentage)
        {
            decimal totalGstDecimal = (cgstPercentage + sgstPercentage) / 100m;
            decimal divisor = 1 + totalGstDecimal;

            if (divisor == 0) return 0;

            decimal baseRate = finalPrice / divisor;
            return Math.Round(baseRate, 2);
        }

    }
}
