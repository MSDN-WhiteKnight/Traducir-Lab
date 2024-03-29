/* Traducir Windows client
 * Copyright (c) 2020,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Traducir.Wpf
{
    public static class DB
    {
        const string DbName = "Traducir.Ru";

        public static string GetConnectionString(bool dbname)
        {
            string con_str = Properties.Settings.Default.CONNECTION_STRING;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(con_str);

            if (dbname) builder.InitialCatalog = DbName;
            else builder.InitialCatalog = string.Empty;

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

        static int RestoreBacpac(string path, out string results)
        {
            // Drop target database, because .bacpac can't be restored to existing DB
            StringBuilder sb = new StringBuilder(500);
            sb.Append("IF DB_ID('" + DbName + "') IS NOT NULL BEGIN ");
            sb.Append("ALTER DATABASE [" + DbName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE ");
            sb.Append("DROP DATABASE [" + DbName + "] END");
            ExecuteSQL(sb.ToString());

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GetConnectionString(true));
            // Workaround TLS connection issues in new SQL Server versions
            builder.TrustServerCertificate = true;
            string connStr = builder.ToString();

            // Restore .bacpac by invoking SqlPackage (it must be installed globally as dotnet tool)
            // https://learn.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "SqlPackage";
            psi.Arguments = "/Action:Import /SourceFile:\"" + path + "\" ";
            psi.Arguments += "/TargetConnectionString:\"" + connStr + "\" ";

            if (CultureInfo.InstalledUICulture != null)
            {
                // Workaround console encoding issues on russian systems
                string culture = CultureInfo.InstalledUICulture.Name;

                if (string.Equals(culture, "ru-ru", StringComparison.OrdinalIgnoreCase))
                {
                    psi.StandardOutputEncoding = Encoding.GetEncoding(866);
                }
            }

            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            Process pr = new Process();

            using (pr)
            {
                pr.StartInfo = psi;
                pr.Start();
                string s = pr.StandardOutput.ReadToEnd(); //получение вывода
                pr.WaitForExit();
                results = s;
                return pr.ExitCode;
            }
        }

        public static async Task RestoreBackup(string path)
        {
            if (string.Equals(Path.GetExtension(path), ".bacpac", StringComparison.OrdinalIgnoreCase))
            {
                string results;
                int res = RestoreBacpac(path, out results);
                Debug.WriteLine(results);

                if (res != 0)
                {
                    throw new Exception("SqlPackage failed with code " + res.ToString());
                }

                return;
            }

            string bak_path = path;
            string temp_path = null;
            bool delete = false;

            if (Path.GetExtension(path) == ".tgz" ||
                Path.GetExtension(path) == ".gz" ||
                Path.GetExtension(path) == ".tar")
            {
                temp_path = await ExtractBackup(path);

                if (temp_path != null)
                {
                    delete = true; //BAK extracted to temp file
                    bak_path = temp_path;
                }
            }

            string dir = "c:\\Traducir\\";

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            StringBuilder sb = new StringBuilder(500);

            try
            {
                sb.Append("IF DB_ID('Traducir.ru') IS NOT NULL ");
                sb.Append("ALTER DATABASE [Traducir.Ru] SET SINGLE_USER WITH ROLLBACK IMMEDIATE ");
                sb.AppendLine();
                sb.Append("RESTORE DATABASE [Traducir.Ru] FROM DISK = N'");
                sb.Append(bak_path);
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
            finally
            {
                if (delete) File.Delete(temp_path);
            }
        }

        public static async Task<string> ExtractBackup(string file)
        {
            MemoryStream ms = new MemoryStream();

            if (Path.GetExtension(file) == ".tgz" || Path.GetExtension(file) == ".gz")
            {
                using (FileStream input = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    await DecompressGZip(input, ms);
                }
            }
            else if (Path.GetExtension(file) == ".tar")
            {
                //no need to decompress
                byte[] data = File.ReadAllBytes(file);
                await ms.WriteAsync(data, 0, data.Length);
            }
            else
            {
                return null; //no need to do anything
            }

            ms.Seek(0, SeekOrigin.Begin);
            string dst = Path.Combine(Path.GetDirectoryName(file),Path.GetFileNameWithoutExtension(file));

            if (Path.GetExtension(file) == ".tgz" || Path.GetExtension(file) == ".tar" || file.EndsWith(".tar.gz"))
            {
                using (FileStream output = new FileStream(dst, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    string name = ExtractTar(ms, output);
                }
            }
            else if (Path.GetExtension(file) == ".gz")
            {
                //no need to extract Tar
                File.WriteAllBytes(dst, ms.ToArray());
            }
            
            return dst;
        }

        public static async Task DecompressGZip(Stream original, Stream decompressed)
        {
            using (GZipStream decompressionStream = new GZipStream(original, CompressionMode.Decompress))
            {
                await decompressionStream.CopyToAsync(decompressed);
            }
        }

        public static async Task DecompressGZip(string file)
        {
            using (FileStream input = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                string dst = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
                dst = dst + ".tar";

                using (FileStream output = new FileStream(dst, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await DecompressGZip(input, output);
                }
            }
        }

        public static string ExtractTar(Stream input, Stream output)
        {
            byte[] buffer = new byte[100];

            while (true)
            {
                input.Read(buffer, 0, 100);
                string name = Encoding.ASCII.GetString(buffer).Trim('\0');

                if (String.IsNullOrWhiteSpace(name)) break;

                input.Seek(24, SeekOrigin.Current);
                input.Read(buffer, 0, 12);

                long size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);

                input.Seek(376L, SeekOrigin.Current);
                                
                if (!name.Equals("./", StringComparison.InvariantCulture))
                {
                    var buf = new byte[size];
                    input.Read(buf, 0, buf.Length);
                    output.Write(buf, 0, buf.Length);
                    return name;
                }

                var pos = input.Position;

                var offset = 512 - (pos % 512);
                if (offset == 512)
                    offset = 0;

                input.Seek(offset, SeekOrigin.Current);
            }

            return String.Empty;
        }

        public static async Task ExtractTar(string file)
        {
            using (FileStream input = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                MemoryStream ms = new MemoryStream();
                string name = ExtractTar(input, ms);
                byte[] data = ms.ToArray();

                string dst = Path.GetDirectoryName(file);
                dst = Path.Combine(dst, name);

                using (FileStream output = new FileStream(dst, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await output.WriteAsync(data, 0, data.Length);
                }
            }
        }
    }
}
