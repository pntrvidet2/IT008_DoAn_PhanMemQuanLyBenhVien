using System;
using System.Collections.Generic;
using DoAn.Models;
using Microsoft.Data.SqlClient;


namespace DoAn.ClassData
{
    public static class DrugDAO
    {
        public static List<Drug> GetAll()
        {
            var list = new List<Drug>();

            using var conn = Database.CreateConnection();
            conn.Open();

            const string sql = @"SELECT Drug_ID, Drug_Name, Drug_Unit, Drug_Price, Stock_Quantity
                                 FROM DRUG
                                 ORDER BY Drug_ID";

            using var cmd = new SqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                list.Add(new Drug
                {
                    Drug_ID = rd["Drug_ID"]?.ToString() ?? "",
                    Drug_Name = rd["Drug_Name"]?.ToString() ?? "",
                    Drug_Unit = rd["Drug_Unit"]?.ToString() ?? "",
                    Drug_Price = Convert.ToDecimal(rd["Drug_Price"]),
                    Stock_Quantity = Convert.ToInt32(rd["Stock_Quantity"])
                });
            }

            return list;
        }

        public static List<Drug> SearchById(string id)
        {
            var list = new List<Drug>();

            using var conn = Database.CreateConnection();
            conn.Open();

            const string sql = @"SELECT Drug_ID, Drug_Name, Drug_Unit, Drug_Price, Stock_Quantity
                                 FROM DRUG
                                 WHERE Drug_ID = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new Drug
                {
                    Drug_ID = rd["Drug_ID"]?.ToString() ?? "",
                    Drug_Name = rd["Drug_Name"]?.ToString() ?? "",
                    Drug_Unit = rd["Drug_Unit"]?.ToString() ?? "",
                    Drug_Price = Convert.ToDecimal(rd["Drug_Price"]),
                    Stock_Quantity = Convert.ToInt32(rd["Stock_Quantity"])
                });
            }

            return list;
        }

        public static bool Exists(string id)
        {
            using var conn = Database.CreateConnection();
            conn.Open();

            const string sql = @"SELECT COUNT(1) FROM DRUG WHERE Drug_ID = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public static void Insert(Drug d)
        {
            using var conn = Database.CreateConnection();
            conn.Open();

            const string sql = @"INSERT INTO DRUG(Drug_ID, Drug_Name, Drug_Unit, Drug_Price, Stock_Quantity)
                                 VALUES (@id, @name, @unit, @price, @qty)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", d.Drug_ID);
            cmd.Parameters.AddWithValue("@name", d.Drug_Name);
            cmd.Parameters.AddWithValue("@unit", d.Drug_Unit);
            cmd.Parameters.AddWithValue("@price", d.Drug_Price);
            cmd.Parameters.AddWithValue("@qty", d.Stock_Quantity);

            cmd.ExecuteNonQuery();
        }
    }
}
