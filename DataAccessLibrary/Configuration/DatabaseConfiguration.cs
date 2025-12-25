using System;
using System.IO;

namespace DataAccessLibrary.Configuration
{
    public static class DatabaseConfiguration
    {
        public static string GetDatabasePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "VoiceLauncher");
            Directory.CreateDirectory(appFolder);
            return Path.Combine(appFolder, "voicelauncher.db");
        }

        public static string GetConnectionString()
        {
            return $"Data Source={GetDatabasePath()}";
        }
    }
}
