using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillManagementSoftware.Repository.DbConfig
{
    public interface IMysqlData
    {
        string GetConnectionString();
        Task<bool> ExecuteNonQueryAsync(string query, MySqlParameter[] mysqlParameterArr = null);
        Task<MySqlDataReader> ExecuteReaderAsync(string query, MySqlParameter[] mysqlParameterArr = null);
        Task<string> ExecuteScalarAsync(string query, MySqlParameter[] mysqlParameterArr = null);

    }
}
