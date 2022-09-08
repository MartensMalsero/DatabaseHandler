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
        /// <param name="sql"></param>
        public DatabaseBuilder(bool debug, Functions functions, string sql)
        {
            _connection = null;
            ConnectionString = DatabaseController.ConnectionStringShare();
            Debug = debug;
            Sql = sql;

            switch (functions)
            {
                case Functions.CreateDatabase:
                    CreateDatabase();
                    break;
                
                case Functions.CreateTable:
                    CreateTable();
                    break;
            }
        }

        private static void CreateDatabase()
        {
            if (!Utils.CheckConnectionString(ConnectionString)) return;
            if (!Utils.CheckSqlString(Sql)) return;

            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();
                MySqlCommand cmd = _connection.CreateCommand();

                cmd.CommandText = Sql;
                
                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                if ((long) cmd.ExecuteScalar() <= 0) return;
                
                _connection.Close();
                Console.WriteLine("Database created!");
            }
        }

        private static void CreateTable()
        {
            if (!Utils.CheckConnectionString(ConnectionString)) return;
            if (!Utils.CheckSqlString(Sql)) return;
            
            using (_connection = new MySqlConnection(ConnectionString))
            {
                _connection.Open();
                MySqlCommand cmd = _connection.CreateCommand();

                cmd.CommandText = Sql;
                
                if (Debug) Console.WriteLine("CMD:COMMANDTEXT -> " + cmd.CommandText);
                if ((long) cmd.ExecuteScalar() <= 0) return;
                
                _connection.Close();
                Console.WriteLine("Database table created!");
            }
        }
    }
}