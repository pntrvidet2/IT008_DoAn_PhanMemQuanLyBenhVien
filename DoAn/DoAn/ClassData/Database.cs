using System.Data;
using Microsoft.Data.SqlClient;

namespace DoAn.ClassData
{
    public class Database
    {
        // CHUỖI KẾT NỐI SQL SERVER
        private const string ConnectionString =
            @"Server=localhost;Database=QLBV;Trusted_Connection=True;TrustServerCertificate=True;";

        // Tạo connection
        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        // SELECT -> DataTable (không param)
        public DataTable GetData(string sql)
        {
            var dt = new DataTable();

            using var conn = CreateConnection();
            using var da = new SqlDataAdapter(sql, conn);
            da.Fill(dt);

            return dt;
        }

        // SELECT -> DataTable (có param)
        public DataTable GetData(string sql, params SqlParameter[] parameters)
        {
            var dt = new DataTable();

            using var conn = CreateConnection();     // ✅ dùng ConnectionString chuẩn
            conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        // SELECT SCALAR
        public object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi ExecuteScalar: " + ex.Message);
                }
            }
        }
        public object GetValue(string sql, params SqlParameter[] parameters)
        {
            using var conn = CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            return cmd.ExecuteScalar();
        }

        // INSERT/UPDATE/DELETE
        public bool Execute(string sql, params SqlParameter[] parameters)
        {
            using var conn = CreateConnection();     // ✅ dùng ConnectionString chuẩn
            conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
