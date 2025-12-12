using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    internal static class DBConnection
    {
        // factory method
        public static SqlConnection GetConnection()
        {
            SqlConnection conn = null;

            string connectionString = "Data Source=localhost;Initial Catalog=patriot_thanks_db;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
            conn = new SqlConnection(connectionString);

            return conn;
        }
    }
}
