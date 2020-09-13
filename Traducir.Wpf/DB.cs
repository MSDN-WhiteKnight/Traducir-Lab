/* Traducir Windows client
 * Copyright (c) 2020,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Traducir.Wpf
{
    public static class DB
    {
        public static string GetConnectionString(bool dbname)
        {
            string con_str = Properties.Settings.Default.CONNECTION_STRING;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(con_str);

            if (dbname) builder.InitialCatalog = "Traducir.Ru";
            else builder.InitialCatalog = "";

            return builder.ConnectionString;
        }

        public static int ExecuteSQL(string query)
        {
            SqlConnection con = new SqlConnection(GetConnectionString(false));
            con.Open();

            using (con)
            {
                SqlCommand cmd = new SqlCommand(query, con);
                return cmd.ExecuteNonQuery();
            }
        }

        public static async Task<int> ExecuteSqlAsync(string query)
        {
            SqlConnection con = new SqlConnection(GetConnectionString(false));
            await con.OpenAsync();

            using (con)
            {
                SqlCommand cmd = new SqlCommand(query, con);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RestoreBackup(string path)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = "c:\\Traducir\\";//Path.Combine(appdata, "Traducir.Wpf");

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            StringBuilder sb = new StringBuilder(500);            
            sb.Append("IF DB_ID('Traducir.ru') IS NOT NULL ");
            sb.Append("ALTER DATABASE [Traducir.Ru] SET SINGLE_USER WITH ROLLBACK IMMEDIATE ");
            sb.AppendLine();
            sb.Append("RESTORE DATABASE [Traducir.Ru] FROM DISK = N'");
            sb.Append(path);
            sb.Append("' WITH FILE = 1, MOVE N'Traducir.Ru' TO N'");                        
            sb.Append(Path.Combine(dir, "Traducir.Ru.mdf"));
            sb.Append("', MOVE N'Traducir.Ru_log' TO N'");                        
            sb.Append(Path.Combine(dir, "Traducir.Ru_Log.ldf"));
            sb.Append("', REPLACE ");
            sb.AppendLine();
            sb.Append("ALTER DATABASE [Traducir.Ru] SET MULTI_USER ");
            sb.AppendLine();
            await ExecuteSqlAsync(sb.ToString());
        }
    }
}
