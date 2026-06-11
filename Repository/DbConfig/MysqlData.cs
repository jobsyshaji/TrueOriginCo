using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BillManagementSoftware.Repository.DbConfig
{
    public class MysqlData : IMysqlData
    {
        public string GetConnectionString() => ConfigurationManager.ConnectionStrings["cherukaraConn"].ConnectionString;

        public async Task<bool> ExecuteNonQueryAsync(string query, MySqlParameter[] mysqlParameterArr = null)
        {
            using (MySqlConnection con = new MySqlConnection(GetConnectionString()))
            {
                await con.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    if (mysqlParameterArr != null)
                    {
                        cmd.Parameters.AddRange(mysqlParameterArr);
                    }
                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
            }
        }

        public async Task<MySqlDataReader> ExecuteReaderAsync(string query, MySqlParameter[] mysqlParameterArr = null)
        {
            MySqlConnection con = new MySqlConnection(GetConnectionString());
            await con.OpenAsync();

            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                if (mysqlParameterArr != null)
                {
                    cmd.Parameters.AddRange(mysqlParameterArr);
                }

                return (MySqlDataReader)await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
        }

        public async Task<string> ExecuteScalarAsync(string query, MySqlParameter[] mysqlParameterArr = null)
        {
            using (MySqlConnection con = new MySqlConnection(GetConnectionString()))
            {
                await con.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    if (mysqlParameterArr != null)
                    {
                        cmd.Parameters.AddRange(mysqlParameterArr);
                    }
                    object result = await cmd.ExecuteScalarAsync();
                    return result != null ? result.ToString() : "";
                }
            }
        }
    }
}