using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace DatabaseHandler
{
    /// <summary>
    ///     Controller for using the database
    /// </summary>
    public class DatabaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DatabaseController CreateInstance()
        {
            return new DatabaseController("localhost", "3306", "", "root", "", false);
        }
        
        #region Variables
        private string Host { get; }
        private string Port { get; }
        private string Db { get; }
        private string Username { get; }
        private string Password { get; }


        private static MySqlConnection? _connection;
        private static bool Debug { get; set; }
        private static string? ConnectionString { get; set; }
        #endregion

        #region Constructor DatabaseController
        /// <summary>
        ///     <example>
        ///         <para>You can set debug to true or false to get console messages or not</para>
        ///         <para>Initialize as follows</para>
        ///         <code>
        ///             _ = new DatabaseController(<paramref name="host"/>, <paramref name="port"/>, <paramref name="db"/>, <paramref name="username"/>, <paramref name="password"/>, <paramref name="debug"/>)
        ///         </code>
        ///     </example>
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="db"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="debug"></param>
        public DatabaseController(string host, string port, string db, string username, string password, bool debug)
        {
            Host = host;
            Port = port;
            Db = db;
            Username = username;
            Password = password;
            Debug = debug;
            ConnectionString = $"SERVER={Host}; PORT={Port}; DATABASE={Db}; UID={Username}; PASSWORD={Password}";
        }

        #endregion

        internal static string? ConnectionStringShare()
        {
            return ConnectionString;
        }

        #region SelectSql
        /// <summary>
        ///     <example>
        ///         <para>Select the <paramref name="columns"/> in the given <paramref name="tableName"/></para>
        ///         <para>If you want to select everything in <paramref name="tableName"/>, e.g. in table "users"</para>
        ///         <code>
        ///             DatabaseController.SelectSQL("users", new object[] {"*"})
        ///         </code>
        ///     </example>
        /// </summary>
        /// 
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="optionalWhere"></param>
        /// <param name="optionalWhereColumns"></param>
        /// <param name="optionalWhereValues"></param>
        /// <param name="optionalLimit"></param>
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
        public static DataTable SelectSql(string tableName, object[] columns, bool optionalWhere = false, object[]? 
        optionalWhereColumns = null, object[]? optionalWhereValues = null, int optionalLimit = 0)
        {
            DataTable dt = new();

            if (!Utils.CheckConnectionString(ConnectionString)) return dt;

            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();
                MySqlCommand cmd = _connection.CreateCommand();

                cmd.CommandText = "SELECT ";

                if (!optionalWhere)
                {
                    for (var i = 1; i <= columns.Length; i++)
                    {
                        if (i != columns.Length)
                        {
                            cmd.CommandText += columns[i - 1] + ", ";
                        }
                        else
                        {
                            if (optionalLimit == 0)
                            {
                                cmd.CommandText += columns[i - 1] + $" FROM {tableName}";
                            }
                            else
                            {
                                cmd.CommandText += columns[i - 1] + $" FROM {tableName} LIMIT @limit";
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 1; i <= columns.Length; i++)
                    {
                        if (i != columns.Length)
                        {
                            cmd.CommandText += columns[i - 1] + ", ";
                        }
                        else
                        {
                            cmd.CommandText += columns[i - 1] + $" FROM {tableName} WHERE ";
                        }
                    }

                    if (optionalWhereColumns != null && optionalWhereValues != null)
                    {
                        for (int i = 1; i <= optionalWhereColumns.Length; i++)
                        {
                            if (i != optionalWhereColumns.Length)
                            {
                                cmd.CommandText += optionalWhereColumns[i - 1] + " LIKE '" + optionalWhereValues[i - 1] + "' AND ";
                            }
                            else
                            {
                                if (optionalLimit == 0)
                                {
                                    cmd.CommandText += optionalWhereColumns[i - 1] + " LIKE '" + optionalWhereValues[i - 1] + "'";
                                }
                                else
                                {
                                    cmd.CommandText += optionalWhereColumns[i - 1] + " LIKE '" + optionalWhereValues[i - 1] + "' LIMIT @limit";
                                }
                            }
                        }
                    }
                }

                if (optionalLimit != 0) cmd.Parameters.AddWithValue("@limit", optionalLimit);

                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                using MySqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
                _connection.Close();
            }

            return dt;
        }
        #endregion

        #region UpsertSQL
        /// <summary>
        ///     To update the <paramref name="columns"/> with <paramref name="values"/> in <paramref name="tableName"/>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <param name="id"></param>
        /// <param name="idColumnName"></param>
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
        public static bool UpsertSql(string tableName, object[] columns, object[] values, int id, string idColumnName = "id")
        {
            var res = false;

            if (!Utils.CheckConnectionString(ConnectionString)) return res;

            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();

                MySqlCommand cmd = _connection.CreateCommand();

                cmd.CommandText = $"UPDATE {tableName} SET ";

                if (columns.Length != values.Length) return res;
                for (var i = 1; i <= columns.Length; i++)
                {
                    if (i != columns.Length)
                    {
                        cmd.CommandText += columns[i - 1] + "=@val" + (i - 1) + ", ";
                    }
                    else
                    {
                        cmd.CommandText += columns[i - 1] + "=@val" + (i - 1) + " WHERE " + idColumnName + "=@id";
                    }
                }

                for (var i = 1; i <= values.Length; i++)
                {
                    cmd.Parameters.AddWithValue("@val" + (i - 1), values[i - 1]);
                }

                cmd.Parameters.AddWithValue("@id", id);

                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                if (cmd.ExecuteNonQuery() > 0)
                {
                    _connection.Close();
                    res = true;
                }
                else
                {
                    //INSERT
                    cmd.CommandText = $"INSERT INTO {tableName} (";

                    for (var i = 1; i <= columns.Length; i++)
                    {
                        if (i != columns.Length)
                        {
                            cmd.CommandText += columns[i - 1] + ", ";
                        }
                        else
                        {
                            cmd.CommandText += columns[i - 1] + ") VALUES (";
                        }
                    }

                    for (var i = 1; i <= values.Length; i++)
                    {
                        if (i != values.Length)
                        {
                            cmd.CommandText += "@val" + (i - 1) + ", ";
                        }
                        else
                        {
                            cmd.CommandText += "@val" + (i - 1) + ")";
                        }
                    }

                    if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                    if (cmd.ExecuteNonQuery() <= 0) return res;
                    _connection.Close();
                    res = true;
                }
            }

            return res;
        }
        #endregion

        #region DeleteSQL
        /// <summary>
        ///     To delete entries or lines
        /// </summary>
        /// <param name="tableName"></param>
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
        public static bool DeleteSql(string tableName, object[] columns, object[] values)
        {
            var res = false;

            if (string.IsNullOrEmpty(ConnectionString))
            {
                Console.WriteLine("ERROR! No connection data set!");
                return res;
            }

            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();
                MySqlCommand cmd = _connection.CreateCommand();

                cmd.CommandText = $"DELETE FROM {tableName} WHERE ";

                if (columns.Length > 0 && values.Length > 0 && columns.Length == values.Length)
                {
                    for (var i = 1; i <= columns.Length; i++)
                    {
                        if (i != columns.Length)
                        {
                            cmd.CommandText += columns[i - 1] + "=@val" + (i - 1) + ", ";
                        }
                        else
                        {
                            cmd.CommandText += columns[i - 1] + "=@val" + (i - 1);
                        }
                    }

                    for (var i = 1; i <= values.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@val" + (i - 1), values[i - 1]);
                    }

                    cmd.Parameters.AddWithValue("@table", tableName);
                }

                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                if (cmd.ExecuteNonQuery() <= 0) return res;
                _connection.Close();
                res = true;

                return res;
            }
        }
        #endregion

        #region CountSQL
        /// <summary>
        ///     Counting the specified entries in the <paramref name="tableName"/>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="optionalWhere"></param>
        /// <param name="optionalWhereColumns"></param>
        /// <param name="optionalWhereValues"></param>
        /// <returns>
        ///     <example>
        ///         <code>
        ///             Int64 count = DatabaseController.CountSQL(...)
        ///         </code>
        ///     </example>
        /// </returns>
        public static long CountSql(string tableName, bool optionalWhere = false, object[]? optionalWhereColumns = 
        null, object[]? optionalWhereValues = null)
        {
            long res = 0;

            if (string.IsNullOrEmpty(ConnectionString))
            {
                Console.WriteLine("ERROR! No connection data set!");
                return res;
            }

            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();
                MySqlCommand cmd = _connection.CreateCommand();

                cmd.CommandText = "SELECT ";

                if (!optionalWhere)
                {
                    cmd.CommandText += $"COUNT(*) FROM {tableName}";
                }
                else
                {
                    if (optionalWhereColumns == null || optionalWhereValues == null) return res;
                    cmd.CommandText += $"COUNT(*) FROM {tableName} WHERE ";

                    if (optionalWhereColumns.Length == optionalWhereValues.Length)
                    {
                        for (var i = 1; i <= optionalWhereColumns.Length; i++)
                        {
                            if (i != optionalWhereColumns.Length)
                            {
                                cmd.CommandText += optionalWhereColumns[i - 1] + "=@val" + (i - 1) + " AND ";
                            }
                            else
                            {
                                cmd.CommandText += optionalWhereColumns[i - 1] + "=@val" + (i - 1) + "";
                            }
                        }

                        for (var i = 1; i <= optionalWhereValues.Length; i++)
                        {
                            cmd.Parameters.AddWithValue("@val" + (i - 1), optionalWhereValues[i - 1]);
                        }
                    }
                }

                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);

                res = (long)cmd.ExecuteScalar();

                _connection.Close();
                return res;
            }
        }
        #endregion

        #region MaxIDSQL
        /// <summary>
        ///     Get last entry in <paramref name="tableName"/> + 1
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="rowName"></param>
        /// <param name="optionalWhere"></param>
        /// <param name="optionalWhereColumns"></param>
        /// <param name="optionalWhereValues"></param>
        /// <returns>
        ///     <example>
        ///         <code>
        ///             Int64 MaxID = DatabaseController.Max_ID_SQL(...)
        ///         </code>
        ///     </example>
        /// </returns>
        public static long MAX_ID_SQL(string tableName, string rowName, bool optionalWhere = false, object[]? 
        optionalWhereColumns = null, object[]? optionalWhereValues = null)
        {
            long res = 0;

            if (string.IsNullOrEmpty(ConnectionString))
            {
                Console.WriteLine("ERROR! No connection data set!");
                return res;
            }

            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();
                MySqlCommand cmd = _connection.CreateCommand();

                cmd.CommandText = "SELECT ";

                if (!optionalWhere)
                {
                    cmd.CommandText += $"MAX({rowName}) FROM {tableName}";
                }
                else
                {
                    cmd.CommandText += $"MAX({rowName}) FROM {tableName} WHERE ";

                    if (optionalWhereColumns == null || optionalWhereValues == null) return res;
                    if (optionalWhereColumns.Length == optionalWhereValues.Length)
                    {
                        for (var i = 1; i <= optionalWhereColumns.Length; i++)
                        {
                            if (i != optionalWhereColumns.Length)
                            {
                                cmd.CommandText += optionalWhereColumns[i - 1] + "=@val" + (i - 1) + " AND ";
                            }
                            else
                            {
                                cmd.CommandText += optionalWhereColumns[i - 1] + "=@val" + (i - 1) + "";
                            }
                        }

                        for (var i = 1; i <= optionalWhereValues.Length; i++)
                        {
                            cmd.Parameters.AddWithValue("@val" + (i - 1), optionalWhereValues[i - 1]);
                        }
                    }
                }

                if (Debug)
                {
                    Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                    Console.WriteLine(cmd.ExecuteScalar().ToString());
                }

                if (!Convert.IsDBNull(cmd.ExecuteScalar()))
                {
                    if (Debug) Console.WriteLine(Convert.ToInt64(cmd.ExecuteScalar()).ToString());
                    res = Convert.ToInt64(cmd.ExecuteScalar());
                    _connection.Close();
                }

                else
                {
                    if (Debug) Console.WriteLine(Convert.ToInt64(0).ToString());
                    _connection.Close();
                }
            }

            return res;
        }
        #endregion
    }
}