using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Npgsql;
//using System.Collections.Generic;
//Para leer escribir archivo JSON
using Newtonsoft.Json;
using System.IO;
//using System.Text;

namespace apppachecograficas
{
    class ConexionPostgres
    {
        private string archivoConfiguracion = "config/servidor.json";
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        // PostgeSQL-style connection string
        private string getConnString()
        {
            StreamReader r = new StreamReader(this.archivoConfiguracion, Encoding.Default, true);
            string json = r.ReadToEnd();
            r.Close();
            dynamic array = JsonConvert.DeserializeObject(json);
            return String.Format(
            "Server={0};" +
            "Port={1};" +
            "User Id={2};" +
            "Password={3};" +
            "Database={4};",
            array["ip"],
            array["puerto"],
            array["usuario"],
            array["clave"],
            array["basededatos"]);
        }
        public List<Dictionary<string, string>> consultar(string sql)
        {
            List<Dictionary<string, string>> filas = new List<Dictionary<string, string>>();
            try
            {
                // Making connection with Npgsql provider
                NpgsqlConnection conn = new NpgsqlConnection(this.getConnString());
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    // Insert some data
                    //cmd.CommandText = "INSERT INTO data (some_field) VALUES ('Hello world')";
                    //cmd.ExecuteNonQuery();
                    // Retrieve all rows
                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, string> columnas = new Dictionary<string, string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                columnas.Add(reader.GetName(i), reader[i].ToString());
                            }
                            filas.Add(columnas);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception msg)
            {
                // something went wrong, and you wanna know why
                Console.WriteLine(msg.ToString());
            }
            return filas;
        }

        public bool registrar(string sql)
        {
            try
            {
                // Making connection with Npgsql provider
                NpgsqlConnection conn = new NpgsqlConnection(this.getConnString());
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                return true;
            }
            catch (Exception msg)
            {
                // something went wrong, and you wanna know why
                return false;
            }
        }
    }
}
