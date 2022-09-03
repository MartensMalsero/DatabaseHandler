using MySql.Data.MySqlClient;
using System.Data;

namespace DatabaseHandler
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseController
    {
        #region Variables
        private string? Host { get; set; }
        private string? Port { get; set; }
        private string? DB { get; set; }
        private string? Username { get; set; }
        private string? Password { get; set; }


        private static MySqlConnection? Connection;
        private static bool _DEBUG { get; set; } = false;
        private static string? ConnectionString { get; set; }
        #endregion

        #region Constructor DatabaseController
        /// <summary>
        ///     <example>
        ///         <para>You can set debug to true or false to get console messages or not</para>
        ///         <para>Initialize as follows</para>
        ///         <code>
        ///             _ = new DatabaseController(<paramref name="Host"/>, <paramref name="Port"/>, <paramref name="DB"/>, <paramref name="Username"/>, <paramref name="Password"/>, <paramref name="DEBUG"/>)
        ///         </code>
        ///     </example>
        /// </summary>
        /// <param name="Host"></param>
        /// <param name="Port"></param>
        /// <param name="DB"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="DEBUG"></param>
        public DatabaseController(string Host, string Port, string DB, string Username, string Password, bool DEBUG)
        {
            this.Host = Host;
            this.Port = Port;
            this.DB = DB;
            this.Username = Username;
            this.Password = Password;
            _DEBUG = DEBUG;
            ConnectionString = $"SERVER={this.Host}; PORT={this.Port}; DATABASE={this.DB}; UID={this.Username}; PASSWORD={this.Password}";
        }
        #endregion

        #region SelectSQL
        /// <summary>
        ///     <example>
        ///         <para>Select the <paramref name="columns"/> in the given <paramref name="tablename"/></para>
        ///         <para>If you want to select everything in <paramref name="tablename"/>, e.g. in table "users"</para>
        ///         <code>
        ///             DatabaseController.SelectSQL("users", new object[] {"*"})
        ///         </code>
        ///     </example>
        /// </summary>
        /// 
        /// <param name="tablename"></param>
        /// <param name="columns"></param>
        /// <param name="optional_where"></param>
        /// <param name="optional_where_columns"></param>
        /// <param name="optional_where_values"></param>
        /// <param name="optional_limit"></param>
        ///
        /// <returns>
        ///     <para>DataTable with all <paramref name="columns"/> and values</para>
        ///     <example>
        ///         You have to catch the result as a DataTable, for example
        ///         <code>
        ///             using DataTable dt = DatabaseController.SelectSQL("users", new object[] {"*"});
        ///             if (dt.Rows.Count > 0)
        ///             {
        ///                 foreach (DataRow dr in dt.Rows)
        ///                 {
        ///                     Console.WriteLine(dr["ID"]);
        ///                 }
        ///             }
        ///         </code>
        ///     </example>
        /// </returns>
        public static DataTable SelectSQL(string tablename, object[] columns, bool optional_where = false, object[]? optional_where_columns = null, object[]? optional_where_values = null, int optional_limit = 0)
        {
            DataTable dt = new();

            if (ConnectionString == null || ConnectionString == "")
            {
                Console.WriteLine("ERROR! No connection data set!");
                return dt;
            }

            using (Connection = new MySqlConnection(ConnectionString))
            {
                Connection.Open();
                MySqlCommand cmd = Connection.CreateCommand();

                cmd.CommandText = "SELECT ";

                if (!optional_where)
                {
                    for (int i = 1; i <= columns.Length; i++)
                    {
                        if (!(i == columns.Length))
                        {
                            cmd.CommandText += columns[i - 1] + ", ";
                        }
                        else
                        {
                            if (optional_limit == 0)
                            {
                                cmd.CommandText += columns[i - 1] + $" FROM {tablename}";
                            }
                            else
                            {
                                cmd.CommandText += columns[i - 1] + $" FROM {tablename} LIMIT @limit";
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= columns.Length; i++)
                    {
                        if (!(i == columns.Length))
                        {
                            cmd.CommandText += columns[i - 1] + ", ";
                        }
                        else
                        {
                            cmd.CommandText += columns[i - 1] + $" FROM {tablename} WHERE ";
                        }
                    }

                    if (optional_where_columns != null && optional_where_values != null)
                    {
                        for (int i = 1; i <= optional_where_columns.Length; i++)
                        {
                            if (!(i == optional_where_columns.Length))
                            {
                                cmd.CommandText += optional_where_columns[i - 1] + " LIKE '" + optional_where_values[i - 1] + "' AND ";
                            }
                            else
                            {
                                if (optional_limit == 0)
                                {
                                    cmd.CommandText += optional_where_columns[i - 1] + " LIKE '" + optional_where_values[i - 1] + "'";
                                }
                                else
                                {
                                    cmd.CommandText += optional_where_columns[i - 1] + " LIKE '" + optional_where_values[i - 1] + "' LIMIT @limit";
                                }
                            }
                        }
                    }
                }

                if (optional_limit != 0) cmd.Parameters.AddWithValue("@limit", optional_limit);

                if (_DEBUG) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                using MySqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
                Connection.Close();
            }

            return dt;
        }
        #endregion

        #region UpsertSQL
        /// <summary>
        ///     To update the <paramref name="columns"/> with <paramref name="values"/> in <paramref name="tablename"/>
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <param name="ID"></param>
        /// <param name="id_column_name"></param>
        /// <returns>
        ///     <example>
        ///         <code>
        ///             bool upsert = DatabaseController.UpsertSQL(...)
        ///             
        ///             if (upsert) {
        ///                 ...
        ///             }
        ///         </code>
        ///     </example>
        /// </returns>
        public static bool UpsertSQL(string tablename, object[] columns, object[] values, int ID, string id_column_name = "id")
        {
            bool res = false;

            if (ConnectionString == null || ConnectionString == "")
            {
                Console.WriteLine("ERROR! No connection data set!");
                return res;
            }

            using (Connection = new MySqlConnection(ConnectionString))
            {
                Connection.Open();

                MySqlCommand cmd = Connection.CreateCommand();

                cmd.CommandText = $"UPDATE {tablename} SET ";

                if (columns.Length == values.Length)
                {
                    for (int i = 1; i <= columns.Length; i++)
                    {
                        if (!(i == columns.Length))
                        {
                            cmd.CommandText += columns[i - 1] + "=@val" + (i - 1) + ", ";
                        }
                        else
                        {
                            cmd.CommandText += columns[i - 1] + "=@val" + (i - 1) + " WHERE " + id_column_name + "=@id";
                        }
                    }

                    for (int i = 1; i <= values.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@val" + (i - 1), values[i - 1]);
                    }

                    cmd.Parameters.AddWithValue("@id", ID);

                    if (_DEBUG) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        Connection.Close();
                        res = true;
                    }
                    else
                    {
                        //INSERT
                        cmd.CommandText = $"INSERT INTO {tablename} (";

                        for (int i = 1; i <= columns.Length; i++)
                        {
                            if (!(i == columns.Length))
                            {
                                cmd.CommandText += columns[i - 1] + ", ";
                            }
                            else
                            {
                                cmd.CommandText += columns[i - 1] + ") VALUES (";
                            }
                        }

                        for (int i = 1; i <= values.Length; i++)
                        {
                            if (!(i == values.Length))
                            {
                                cmd.CommandText += "@val" + (i - 1) + ", ";
                            }
                            else
                            {
                                cmd.CommandText += "@val" + (i - 1) + ")";
                            }
                        }

                        /*for(int i = 1; i <= values.Length; i++)
                        {
                            cmd.Parameters.AddWithValue("@val" + (i - 1), values[i - 1]);
                        }*/

                        if (_DEBUG) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                        if (cmd.ExecuteNonQuery() > 0)
                        {   
                            Connection.Close();
                            res = true;
                        }
                    }
                }
            }

            return res;
        }
        #endregion

        #region DeleteSQL
        /// <summary>
        ///     To delete entries or lines
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <returns>
        ///     <example>
        ///         <code>
        ///             bool delete = DatabaseController.DeleteSQL(...)
        ///             
        ///             if (delete) {
        ///                 ...
        ///             }
        ///         </code>
        ///     </example>
        /// </returns>
        public static bool DeleteSQL(string tablename, object[] columns, object[] values)
        {
            bool res = false;

            if (ConnectionString == null || ConnectionString == "")
            {
                Console.WriteLine("ERROR! No connection data set!");
                return res;
            }

            using (Connection = new MySqlConnection(ConnectionString))
            {
                Connection.Open();
                MySqlCommand cmd = Connection.CreateCommand();

                cmd.CommandText = $"DELETE FROM {tablename} WHERE ";

                if (columns.Length > 0 && values.Length > 0 && columns.Length == values.Length)
                {
                    for (int i = 1; i <= columns.Length; i++)
                    {
                        if (!(i == columns.Length))
                        {
                            cmd.CommandText += columns[i - 1] + "=@val" + (i - 1) + ", ";
                        }
                        else
                        {
                            cmd.CommandText += columns[i - 1] + "=@val" + (i - 1);
                        }
                    }

                    for (int i = 1; i <= values.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@val" + (i - 1), values[i - 1]);
                    }

                    cmd.Parameters.AddWithValue("@table", tablename);
                }

                if (_DEBUG) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                if (cmd.ExecuteNonQuery() > 0)
                {
                    Connection.Close();
                    res = true;
                }

                return res;
            }
        }
        #endregion

        #region CountSQL
        /// <summary>
        ///     Counting the specified entries in the <paramref name="tablename"/>
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="optional_where"></param>
        /// <param name="optional_where_columns"></param>
        /// <param name="optional_where_values"></param>
        /// <returns>
        ///     <example>
        ///         <code>
        ///             Int64 count = DatabaseController.CountSQL(...)
        ///         </code>
        ///     </example>
        /// </returns>
        public static Int64 CountSQL(string tablename, bool optional_where = false, object[] optional_where_columns = null, object[] optional_where_values = null)
        {
            Int64 res = 0;

            if (ConnectionString == null || ConnectionString == "")
            {
                Console.WriteLine("ERROR! No connection data set!");
                return res;
            }

            using (Connection = new MySqlConnection(ConnectionString))
            {
                Connection.Open();
                MySqlCommand cmd = Connection.CreateCommand();

                cmd.CommandText = "SELECT ";

                if (!optional_where)
                {
                    cmd.CommandText += $"COUNT(*) FROM {tablename}";
                }
                else
                {
                    cmd.CommandText += $"COUNT(*) FROM {tablename} WHERE ";

                    if (optional_where_columns.Length == optional_where_values.Length)
                    {
                        for (int i = 1; i <= optional_where_columns.Length; i++)
                        {
                            if (!(i == optional_where_columns.Length))
                            {
                                cmd.CommandText += optional_where_columns[i - 1] + "=@val" + (i - 1) + " AND ";
                            }
                            else
                            {
                                cmd.CommandText += optional_where_columns[i - 1] + "=@val" + (i - 1) + "";
                            }
                        }

                        for (int i = 1; i <= optional_where_values.Length; i++)
                        {
                            cmd.Parameters.AddWithValue("@val" + (i - 1), optional_where_values[i - 1]);
                        }
                    }
                }

                if (_DEBUG) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                res = (Int64)cmd.ExecuteScalar();

                Connection.Close();
                return res;
            }
        }
        #endregion

        #region MaxIDSQL
        /// <summary>
        ///     Get last entry in <paramref name="tablename"/> + 1
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="rowname"></param>
        /// <param name="optional_where"></param>
        /// <param name="optional_where_columns"></param>
        /// <param name="optional_where_values"></param>
        /// <returns>
        ///     <example>
        ///         <code>
        ///             Int64 MaxID = DatabaseController.Max_ID_SQL(...)
        ///         </code>
        ///     </example>
        /// </returns>
        public static Int64 MAX_ID_SQL(string tablename, string rowname, bool optional_where = false, object[] optional_where_columns = null, object[] optional_where_values = null)
        {
            Int64 res = 0;

            if (ConnectionString == null || ConnectionString == "")
            {
                Console.WriteLine("ERROR! No connection data set!");
                return res;
            }

            using (Connection = new MySqlConnection(ConnectionString))
            {
                Connection.Open();
                MySqlCommand cmd = Connection.CreateCommand();

                cmd.CommandText = "SELECT ";

                if (!optional_where)
                {
                    cmd.CommandText += $"MAX({rowname}) FROM {tablename}";
                }
                else
                {
                    cmd.CommandText += $"MAX({rowname}) FROM {tablename} WHERE ";

                    if (optional_where_columns.Length == optional_where_values.Length)
                    {
                        for (int i = 1; i <= optional_where_columns.Length; i++)
                        {
                            if (!(i == optional_where_columns.Length))
                            {
                                cmd.CommandText += optional_where_columns[i - 1] + "=@val" + (i - 1) + " AND ";
                            }
                            else
                            {
                                cmd.CommandText += optional_where_columns[i - 1] + "=@val" + (i - 1) + "";
                            }
                        }

                        for (int i = 1; i <= optional_where_values.Length; i++)
                        {
                            cmd.Parameters.AddWithValue("@val" + (i - 1), optional_where_values[i - 1]);
                        }
                    }
                }

                if (_DEBUG)
                {
                    Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                    Console.WriteLine(cmd.ExecuteScalar().ToString());
                }

                if (!Convert.IsDBNull(cmd.ExecuteScalar()))
                {
                    if (_DEBUG) Console.WriteLine(Convert.ToInt64(cmd.ExecuteScalar()).ToString());
                    res = Convert.ToInt64(cmd.ExecuteScalar());
                    Connection.Close();
                }

                else
                {
                    if (_DEBUG) Console.WriteLine(Convert.ToInt64(0).ToString());
                    Connection.Close();
                }
            }

            return res;
        }
        #endregion
    }
}