using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatLib
{
    public class MySQLConnectionManager
    {
        public static MySQLConnectionManager con_man;
        private List<MySqlConnection> con_used = new List<MySqlConnection>();
        public static string str_nb_con = ConfigurationManager.AppSettings["mysql_natlib_nb_max_con"];
        public static string str_con = ConfigurationManager.ConnectionStrings["mysql_natlib_str_con"].ConnectionString;

        private MySqlConnection _GetConnection(string sc = "") 
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
                    if (con.State.Equals(System.Data.ConnectionState.Open))
                    {
                        con_used.Add(con);
                        return con;
                    }
                }
            }

            return con;
        }

        private void _RemoveConnection(MySqlConnection con)
        {
            if (con != null)
            {
                if (con.State.Equals(System.Data.ConnectionState.Open))
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
        /// Get connection
        /// </summary>
        /// <param name="sc">string connection</param>
        /// <returns>MySqlConnection</returns>
        public static MySqlConnection GetConnection(string sc = "")
        {
            try
            {
                if (con_man == null) con_man = new MySQLConnectionManager();
                return con_man._GetConnection(sc);
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// RemoveConnection
        /// </summary>
        /// <param name="con">MySlqConnection</param>
        public static void RemoveConnection(MySqlConnection con) 
        {
            try { if (con_man != null) con_man._RemoveConnection(con); }
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

            try
            {
                while (!stop)
                {
                    con = GetConnection(sc);
                    if (con != null && con.State.Equals(System.Data.ConnectionState.Open)) { stop = true; }
                }
            }
            catch (Exception) { return null; }

            return con;
        }
    }
}
