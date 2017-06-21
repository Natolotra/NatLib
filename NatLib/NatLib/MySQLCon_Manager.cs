using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatLib
{
    public static class MySQLCon_Manager
    {
        private static List<MySqlConnection> con_used = new List<MySqlConnection>();
        private static string str_nb_con = ConfigurationManager.AppSettings["mysql_natlib_nb_max_con"];
        private static string str_con = ConfigurationManager.ConnectionStrings["mysql_natlib_str_con"].ConnectionString;

        /// <summary>
        /// Get connection
        /// </summary>
        /// <param name="sc">string connection</param>
        /// <returns>MySqlConnection</returns>
        public static MySqlConnection GetConnection(string sc = "")
        {
            MySqlConnection con = null;
            int int_nb_con = 100;

            try { int_nb_con = int.Parse(str_nb_con); }
            catch (Exception) { int_nb_con = 100; }

            lock (con_used)
            {
                if (con_used.Count < int_nb_con)
                {
                    if (sc != null && sc != "")
                        con = new MySqlConnection(sc);
                    else
                        con = new MySqlConnection(str_con);
                    try { con.Open(); }
                    catch (Exception) { }
                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con_used.Add(con);
                        return con;
                    }
                }
            }
            return con;
        }

        /// <summary>
        /// RemoveConnection
        /// </summary>
        /// <param name="con">MySlqConnection</param>
        public static void RemoveConnection(MySqlConnection con)
        {
            if (con != null)
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    try
                    {
                        con.Close();
                        con.Dispose();
                    }
                    catch (Exception) { }
                }
            }

            try
            {
                lock (con_used)
                {
                    con_used.Remove(con);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// GetPersistConnection
        /// </summary>
        /// <param name="sc">String connection</param>
        /// <returns>MySqlConnection</returns>
        public static MySqlConnection GetPersistConnection(string sc = "")
        {
            MySqlConnection con = null;
            bool stop = false;

            while (!stop)
            {
                con = GetConnection(sc);
                if (con != null && con.State == System.Data.ConnectionState.Open) { stop = true; }
            }

            return con;
        }
    }
}
