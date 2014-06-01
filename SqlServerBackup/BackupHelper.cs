using System;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServerBackup
{
    public static class BackupHelper
    {
        public static void BackupDatabase(Server server, String databaseName, String destinationPath, int completionCallbackInterval, Action<int, String> completionCallback, Action<SqlError> errorCallback)
        {
            String backupSetName = String.Format("EnsembleBackup_{0}", databaseName);
            String backupSetDescription = "User-requested backup for Self-Service Tools";

            //Define a Backup object variable. 
            Backup bk = new Backup
            {
                Action = BackupActionType.Database,
                BackupSetDescription = backupSetDescription,
                BackupSetName = backupSetName,
                Database = databaseName,
                Incremental = false, // This is a full database backup.
                ExpirationDate = System.DateTime.UtcNow.AddYears(-1), // Already expired.  Allows us to overwrite them easily with subsequent backups.
                LogTruncation = BackupTruncateLogType.NoTruncate, // I'm not sure what the implications of truncating the log are, so don't do that.
                CopyOnly = true,
                PercentCompleteNotification = completionCallbackInterval,
                Initialize = true,
            };

            // We're going to save this backup as a file. 
            String backupDeviceName = String.Format(destinationPath, databaseName);
            BackupDeviceItem bdi = new BackupDeviceItem(backupDeviceName, DeviceType.File);
            bk.Devices.Add(bdi);

            //bk.Information += (sender, args) => { bk.Devices.Remove(bdi); errorCallback(args.Error); };
            bk.PercentComplete += (sender, args) => { bk.Devices.Remove(bdi); completionCallback(args.Percent, args.Message); };

            bk.SqlBackup(server);
            Console.WriteLine("Done");
        }
    }
}
