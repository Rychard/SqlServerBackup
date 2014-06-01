using System;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServerBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var results = CommandLine.Parser.Default.ParseArguments(args, options);

            // Exit if arguments couldn't be parsed.
            if (!results) { return; }

            Console.WriteLine("Arguments Parsed!");

            //Connect to the local, default instance of SQL Server. 
            var _server = new Server(@".\SQLExpress");

            foreach (Database database in _server.Databases)
            {
                if (database.Name == "tempdb") { continue; } // Backup and Restore operations are not permitted on tempdb
                BackupHelper.BackupDatabase(_server, database.Name, String.Format("D:\\{0}.bak", database.Name), 1, CompletionCallback, ErrorCallback);
            }
        }

        private static void ErrorCallback(SqlError sqlError)
        {
            Console.WriteLine(sqlError.Message);
        }

        private static void CompletionCallback(int i, string s)
        {
            Console.WriteLine("{0}% - {1}", i, s);
        }
    }

    
}
