using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;

namespace SqlServerBackup
{
    class Program
    {
        private static Settings _settings = null;
        private static List<String> _filesToUpload = null;

        static void Main(string[] args)
        {
            String currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            String settingsFile = Path.Combine(currentDirectory, "Settings.json");

            Boolean settingsFileExists = File.Exists(settingsFile);

            _settings = new Settings();

            if (!settingsFileExists)
            {
                Console.WriteLine("Support does not exist for launching with command-line arguments.");
                Console.WriteLine("A default settings file has been generated.  It can be found at the following location: \n{0}", settingsFile);

                String defaultSettingsSerialized = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(settingsFile, defaultSettingsSerialized);

                return;

                // TODO: Enable support for command-line arguments.

                //var options = new Options();
                //var results = CommandLine.Parser.Default.ParseArguments(args, options);

                //// Exit if arguments couldn't be parsed.
                //if (!results) { return; }


                //Console.WriteLine("Arguments Parsed!");
            }

            try
            {
                String settingsFileContents = File.ReadAllText(settingsFile);
                _settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(settingsFileContents);
            }
            catch (Exception ex)
            {
                // Rethrow the exception for now.
                // In the future we should output a more informative error message.
                throw;
            }

            Server server;

            if (String.IsNullOrEmpty(_settings.Username) || String.IsNullOrEmpty(_settings.Password))
            {
                server = new Server(_settings.Server);
            }
            else
            {
                ServerConnection connection = new ServerConnection(_settings.Server, _settings.Username, _settings.Password);
                server = new Server(connection);
            }
            
            foreach (Database database in server.Databases)
            {
                Boolean shouldBackup = (_settings.BackupAll || _settings.Databases.Contains(database.Name));
                if (shouldBackup)
                {
                    Console.WriteLine("Attempting to backup database: {0}", database.Name);
                }
                else
                {
                    Console.WriteLine("Skipping: {0}", database.Name);
                    continue;
                }

                // Backup operations are not permitted on the "tempdb" database.
                if (database.Name == "tempdb")
                {
                    Console.WriteLine("Backup operations are not permitted on this database.");
                    continue;
                }
                
                // TODO: Check the size of the database.  Ensure that there is enough free space to store the backup.
                Double databaseSize = database.Size;

                Boolean outputDirectoryExists = Directory.Exists(_settings.OutputPath);
                if (!outputDirectoryExists)
                {
                    Directory.CreateDirectory(_settings.OutputPath);
                }

                // TODO: Make configurable.
                String backupFilename = String.Format("{0}.bak", database.Name);

                String outputFilePath = Path.Combine(_settings.OutputPath, backupFilename);

                // Notifications will be raised every _x_ percent.  A value of 1 raises notifications at every percentage.
                const int completionNotificationInterval = 25;

                // TODO: Raise event prior to database backup.

                BackupHelper.BackupDatabase(server, database.Name, outputFilePath, completionNotificationInterval, CompletionCallback, ErrorCallback);

                // TODO: Raise event after database has been backed up.
                
                Console.WriteLine("Backup was saved to the following location:\n{0}", outputFilePath);

                Boolean outputFileExists = File.Exists(outputFilePath);

                Boolean fileCompressedSuccessfully = false;

                String backupFilenameZip = String.Format("{0}_{1}.zip", database.Name, DateTime.UtcNow.ToUnixTimestamp(false));
                String outputFilePathZip = Path.Combine(_settings.OutputPath, backupFilenameZip);
                Boolean outputFilePathZipExists = File.Exists(outputFilePathZip);

                if (outputFilePathZipExists)
                {
                    // Delete the existing zip archive.
                    File.Delete(outputFilePathZip);
                }

                if (outputFileExists)
                {
                    Console.Write("Compressing...");
                    
                    // Keys = Archive Paths
                    // Values = Local File-System Paths
                    Dictionary<String, String> zipContents = new Dictionary<String, String>();
                    zipContents.Add(backupFilename, outputFilePath);
                    ZipHelper.CreateZip(outputFilePathZip, zipContents);
                    Console.WriteLine("Done!  Compressed file is located at the following location:");
                    Console.WriteLine(outputFilePathZip);
                    fileCompressedSuccessfully = true;
                    
                    // Delete the uncompressed file.  We don't need it anymore.
                    File.Delete(outputFilePath);
                }
                else
                {
                    Console.WriteLine("Output file not found!  Unable to compress.");
                }

                if (fileCompressedSuccessfully)
                {
                    if (_filesToUpload == null)
                    {
                        _filesToUpload = new List<String>();
                    }

                    _filesToUpload.Add(outputFilePathZip);
                }
            }

            Console.WriteLine("Database Backups are complete.");

            Console.WriteLine("Uploading Backups to Amazon S3.");

            UploadFiles();

            Console.WriteLine("Uploaded Backups to Amazon S3.");
            Console.WriteLine("All tasks have completed successfully.");
            Console.ReadKey();
        }


        private static Boolean _uploadInProgress = false;
        private static void UploadFiles()
        {
            if (_filesToUpload == null) { return; }

            AmazonS3Helper amazonClient = new AmazonS3Helper(_settings.AwsKeyPublic, _settings.AwsKeySecret, _settings.AwsKeyBucket);

            foreach (var fileToUpload in _filesToUpload)
            {
                while (_uploadInProgress)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                String filename = Path.GetFileName(fileToUpload);
                Console.WriteLine("Uploading: {0}", fileToUpload);
                amazonClient.UploadComplete += AmazonClientOnUploadComplete;
                _uploadInProgress = true;
                amazonClient.UploadFile(filename, fileToUpload);
            }
        }

        private static void AmazonClientOnUploadComplete(object sender, EventArgs eventArgs)
        {
            _uploadInProgress = false;
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
