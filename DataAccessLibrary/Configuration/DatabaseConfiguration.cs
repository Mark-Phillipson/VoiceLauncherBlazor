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
            try
            {
                Directory.CreateDirectory(appFolder);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is ArgumentException ||
                ex is PathTooLongException ||
                ex is NotSupportedException ||
                ex is System.Security.SecurityException)
            {
                throw new InvalidOperationException(
                    $"Failed to create application data folder at '{appFolder}'. " +
                    "Ensure you have write permissions and that the path is valid.",
                    ex);
            }
            return Path.Combine(appFolder, "voicelauncher.db");
        }

        public static string GetConnectionString()
        {
            return $"Data Source={GetDatabasePath()}";
        }
    }
}
