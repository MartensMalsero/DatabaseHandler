using System;

namespace DatabaseHandler
{ 
    internal class Utils
    {
        public static Utils CreateInstance()
        {
            return new Utils();
        }

        internal static bool CheckConnectionString(string? connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString)) return true;
            Console.WriteLine("ERROR! No connection data set!");
            return false;
        }

        internal static bool CheckSqlString(string? sqlString)
        {
            if (!string.IsNullOrEmpty(sqlString)) return true;
            Console.WriteLine("ERROR! No SQL query set!");
            return false;
        }
    }
}