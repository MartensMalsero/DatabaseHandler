using System;
using MySql.Data.MySqlClient;


namespace DatabaseHandler
{
    /// <summary>
    ///     Creating databases and database tables
    /// </summary>
    public class DatabaseBuilder
    {
        #region Variables
        
        /// <summary>
        ///     Functions to call
        /// </summary>
        public enum Functions
        {
            /// <summary>
            ///     Call the function to create the database
            /// </summary>
            CreateDatabase,
            /// <summary>
            ///     Call the function to create database tables
            /// </summary>
            CreateTable
        }
        private static MySqlConnection? _connection;
        private static string? ConnectionString { get; set; }
        private static string? Sql { get; set; }
        private static bool Debug { get; set; }

        #endregion

        /// <summary>
        ///     DatabaseBuilder constructor - call to execute DatabaseBuilder.Functions with the necessary SQL
        /// </summary>
        /// <param name="debug"></param>
        /// <param name="functions"></param>
        /// <param name="name">Database oder table name</param>
        /// <param name="sql"></param>
        public DatabaseBuilder(bool debug, Functions functions, string name, string sql)
        {
            _connection = null;
            ConnectionString = DatabaseController.ConnectionStringShare();
            Debug = debug;
            Sql = sql;

            switch (functions)
            {
                case Functions.CreateDatabase:
                    CreateDatabase(name);
                    break;
                
                case Functions.CreateTable:
                    CreateTable(name);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(functions), functions, null);
            }
        }

        private static void CreateDatabase(string name)
        {
            if (!Utils.CheckConnectionString(ConnectionString)) return;
            if (!Utils.CheckSqlString(Sql)) return;

            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();
                MySqlCommand cmd = _connection.CreateCommand();

                string countDatabase = @$"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.SCHEMATA
                    WHERE SCHEMA_NAME = '{name}'";

                cmd.CommandText = countDatabase;
                
                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                if ((long) cmd.ExecuteScalar() > 0) return;
                
                cmd.CommandText = Sql;
                
                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                cmd.ExecuteNonQuery();
                
                cmd.CommandText = countDatabase;
                    
                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                if ((long) cmd.ExecuteScalar() <= 0) throw new Exception();
                    
                Console.WriteLine("Database created!");
                _connection.Close();
            }
        }

        private static void CreateTable(string name)
        {
            if (!Utils.CheckConnectionString(ConnectionString)) return;
            if (!Utils.CheckSqlString(Sql)) return;

            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();
                MySqlCommand cmd = _connection.CreateCommand();

                string countTable = @$"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE (TABLE_NAME = '{name}')";

                cmd.CommandText = countTable;
                
                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                if ((long) cmd.ExecuteScalar() > 0) return;
                
                cmd.CommandText = Sql;
                
                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                cmd.ExecuteNonQuery();

                cmd.CommandText = countTable;
                    
                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                if ((long) cmd.ExecuteScalar() <= 0) throw new Exception();
                    
                Console.WriteLine("Database table created!");
                _connection.Close();
            }
        }
    }
}