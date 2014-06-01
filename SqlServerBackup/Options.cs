using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace SqlServerBackup
{
    public class Options
    {
        [Option('s', "server", Required = true, HelpText = "Host, IP address, or instance to connect to")]
        public String Server { get; set; }

        [Option('u', "username", Required = true, HelpText = "Username used to authenticate with the server.")]
        public String Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password used to authenticate with the server.")]
        public String Password { get; set; }

        [Option('a', "all", HelpText = "Backup all available databases.")]
        public Boolean BackupAll { get; set; }

        [Option('d', "databases", HelpText = "Database names to backup.")]
        public IEnumerable<String> Databases { get; set; }

        [Option('v', "verbose", HelpText = "Prints all messages to standard output.")]
        public Boolean Verbose { get; set; }

        [Option('o', "output", HelpText = "Path where backups will be stored.")]
        public String OutputPath { get; set; }

        [Option('c', "compress", HelpText = "Enable compression of backups.")]
        public String CompressionFormat { get; set; }

        [HelpOption('?', "help")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("SQL Server Backup", "0.0.0.0"),
                Copyright = new CopyrightInfo("Joshua Shearer", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            //help.AddPreOptionsLine("<<license details here.>>");
            help.AddPreOptionsLine("\nUsage: SqlServerBackup -s \".\\SQLExpress\" --all");
            help.AdditionalNewLineAfterOption = false;
            help.AddOptions(this);
            return help;
        }
    }
}
