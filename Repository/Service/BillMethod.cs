using BillManagementSoftware.Repository.DbConfig;
using BillManagementSoftware.Repository.Interface;
using CherukarasThejas.Models.Response;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BillManagementSoftware.Repository.Service
{
    public class BillMethod : IBillMethod
    {
        private readonly IMysqlData _mysqlData;
        public BillMethod(IMysqlData mysqlData)
        {
            _mysqlData = mysqlData;
        }
        public async Task<BaseResponse<Empty>> SaveBillInfo()
        {
            BaseResponse<Empty> response = new BaseResponse<Empty>();
            try
            {

            }
            catch (Exception)
            {
            }
            return response;
        }

        public async Task<string> GetCustomerName(string mobileNo)
        {
            try
            {
                string query = $"SELECT Name FROM customerinfo where MobileNumber = @MobileNumber;";
                MySqlParameter[] param = { new MySqlParameter("@MobileNumber", mobileNo) };
                
                return await _mysqlData.ExecuteScalarAsync(query, param);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}