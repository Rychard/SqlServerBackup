using Microsoft.SqlServer.Management.Smo;
using System;
using System.Data.SqlClient;

namespace SqlServerBackup
{
	public static class BackupHelper
	{
		// TODO: Abstract parameters into class.
		public static void BackupDatabase(Server server, String databaseName, String destinationPath, int completionCallbackInterval, Action<int, String> completionCallback, Action<SqlError> errorCallback)
		{
			// TODO: Expose as parameter.
			String backupSetName = String.Format("Backup_{0}", databaseName);

			// TODO: Expose as parameter.
			String backupSetDescription = "User-requested backup for Self-Service Tools";

			// Define a Backup object variable.
			var bk = new Backup
			{
				Action = BackupActionType.Database,
				BackupSetDescription = backupSetDescription,
				BackupSetName = backupSetName,
				Database = databaseName,
				Incremental = false, // This is a full database backup.
				ExpirationDate = DateTime.UtcNow.AddYears(-1), // Already expired.  Allows us to overwrite them easily with subsequent backups.
				LogTruncation = BackupTruncateLogType.NoTruncate, // I'm not sure what the implications of truncating the log are, so don't do that.
				CopyOnly = true,
				PercentCompleteNotification = completionCallbackInterval,
				Initialize = true,
			};

			// We're going to save this backup as a file.
			String backupDeviceName = String.Format(destinationPath, databaseName);
			var bdi = new BackupDeviceItem(backupDeviceName, DeviceType.File);
			bk.Devices.Add(bdi);

			//bk.Information += (sender, args) => { bk.Devices.Remove(bdi); errorCallback(args.Error); };
			bk.PercentComplete += (sender, args) => { bk.Devices.Remove(bdi); completionCallback(args.Percent, args.Message); };

			bk.SqlBackup(server);
		}
	}
}