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

            // TODO: Pull from configuration file OR command-line arguments.
            var server = new Server(@".\SQLExpress");

            // TODO: Pull from configuration file OR command-line arguments.
            foreach (Database database in server.Databases)
            {
                // Backup operations are not permitted on the "tempdb" database.
                if (database.Name == "tempdb") { continue; }

                // TODO: Check the size of the database.  Ensure that there is enough free space to store the backup.

                // TODO: Pull from configuration file OR command-line arguments.
                String outputFilepath = String.Format("D:\\{0}.bak", database.Name);

                // TODO: Pull from configuration file OR command-line arguments.
                // Notifications will be raised every _x_ percent.  A value of 1 raises notifications at every percentage.
                int completionNotificationInterval = 1;

                // TODO: Raise event prior to database backup.
                
                BackupHelper.BackupDatabase(server, database.Name, outputFilepath, completionNotificationInterval, CompletionCallback, ErrorCallback);

                // TODO: Raise event after database has been backed up.
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
