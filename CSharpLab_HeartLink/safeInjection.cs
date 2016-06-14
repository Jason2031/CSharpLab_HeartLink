using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MySQLDriverCS;

namespace ConsoleApplication1
{
    class Program
    {
        public static MySQLConnection conn;
        public static MySQLCommand cmd;
        public static MySQLDataAdapter da;

        public Program()
        {
            conn = new MySQLConnection(new MySQLConnectionString("10.125.103.139", "heartlink", "qwe", "123").AsString);
            cmd = new MySQLCommand();
            da = new MySQLDataAdapter();
        }

        public DataTable QeuryTable(string sqlTxt)
        {
            DataTable dt = new DataTable();
            try
            {
                conn.Open();
                cmd.CommandText = sqlTxt;
                cmd.Connection = conn;
                da.SelectCommand = cmd;
                da.Fill(dt);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return dt;
        }
        static void Main(string[] args)
        {
            conn = new MySQLConnection(new MySQLConnectionString("10.125.103.139", "heartlink", "qwe", "123").AsString);
            cmd = new MySQLCommand();
            da = new MySQLDataAdapter();

            String pass = "12345";
            String sql = "select ID from students where password=" + pass;
            
            conn.Open();
            cmd = new MySQLCommand(sql, conn);
            MySQLDataReader DBReader = cmd.ExecuteReaderEx();
            try
            {
                while (DBReader.Read())
                {
                    Console.WriteLine("ID = {0}", DBReader.GetString(0));
                }
            }
            finally
            {
                DBReader.Close();
                conn.Close();
            }

        }
    }
}
