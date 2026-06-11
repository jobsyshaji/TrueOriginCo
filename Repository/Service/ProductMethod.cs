using BillManagementSoftware.Repository.DbConfig;
using BillManagementSoftware.Repository.Interface;
using CherukarasThejas.Areas.BillSoftware.Data;
using CherukarasThejas.Models.Response;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BillManagementSoftware.Repository.Service
{
    public class ProductMethod : IProductMethod
    {
        private readonly IMysqlData _mysqlData;
        public ProductMethod(IMysqlData mysqlData) { _mysqlData = mysqlData; }

        public async Task<BaseResponse<Empty>> SaveProduct(ProductPost data)
        {
            BaseResponse<Empty> response = new BaseResponse<Empty>();
            try
            {
                string query = "";
                if (data.PId == null || data.PId == 0)
                {
                    query = $"insert into productmaster(PNameEN, Rate, AvailableStock, PQuantity, Indate) values( @PNameEN, @Rate, @AvailableStock, @PQuantity, NOW())";
                    response.Message = "Product saved successfully.";
                }
                else
                {
                    query = $"update productmaster SET PNameEN = @PNameEN, Rate = @Rate, AvailableStock = @AvailableStock, PQuantity = @PQuantity  where pid = @pid";
                    response.Message = "Product updated successfully.";
                }
                List<MySqlParameter> parameters = new List<MySqlParameter> {
                    new MySqlParameter("@pid", data.PId),
                    new MySqlParameter("@PNameEN", data.PName),
                    new MySqlParameter("@Rate", data.Rate),
                    new MySqlParameter("@AvailableStock", data.AvailableStock),
                    new MySqlParameter("@PQuantity", data.PQuantity),
                };
                await _mysqlData.ExecuteNonQueryAsync(query, parameters.ToArray());
                response.Status = true;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<BaseResponse<Empty>> DeleteProduct(int pId)
        {
            BaseResponse<Empty> response = new BaseResponse<Empty>();
            try
            {
                if (pId > 0)
                {
                    string query = "DELETE FROM productmaster WHERE PId =@PId";
                    List<MySqlParameter> param = new List<MySqlParameter>
                    {
                        new MySqlParameter("@PId", pId)
                    };
                    await _mysqlData.ExecuteNonQueryAsync(query, param.ToArray());
                    response.Status = true;
                    response.Message = "Product deleted successfully.";
                }
                else
                {
                    response.Status = false; response.Message = "Invalid Product";
                }
            }
            catch (Exception ex)
            {
                response.Status = false; response.Message = ex.Message;
            }
            return response;
        }

        public async Task<List<SelectListItem>> GetProductDrpList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                string query = "select PId, PNameEN from productmaster";
                using (MySqlDataReader reader = await _mysqlData.ExecuteReaderAsync(query))
                {
                    if (reader != null && reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new SelectListItem
                            {
                                Value = reader["PId"].ToString(),
                                Text = reader["PNameEN"].ToString(),
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return list;
        }

        public async Task<string> GetProductRate(int pId)
        {
            try
            {
                string query = "select Rate from productmaster where PId=@PId;";
                MySqlParameter[] param = { new MySqlParameter("@PId", pId) };
                return await _mysqlData.ExecuteScalarAsync(query, param);   
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}