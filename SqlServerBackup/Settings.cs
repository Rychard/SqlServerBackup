using System;
using System.Collections.Generic;

namespace SqlServerBackup
{
    public class Settings
    {
        public String Server { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public Boolean BackupAll { get; set; }
        public String OutputPath { get; set; }
        public IEnumerable<String> Databases { get; set; }
        public String AwsKeyPublic { get; set; }
        public String AwsKeySecret { get; set; }
        public String AwsKeyBucket { get; set; }

        public Settings()
        {
            Server = ".\\SQLExpress";
            Username = "";
            Password = "";
            BackupAll = false;
            OutputPath = AppDomain.CurrentDomain.BaseDirectory;
            Databases = new List<String>();
        }
    }
}
