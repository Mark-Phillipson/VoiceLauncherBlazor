using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataMigration
{
    public static class MigrationHelper
    {
        public static async Task ApplyMigrationsAsync(DbContext context)
        {
            try
            {
                await context.Database.MigrateAsync();
                Console.WriteLine("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying migrations: {ex.Message}");
                // Handle specific errors here
            }
        }
        
        public static async Task<bool> EnsureBridgeTableExistsAsync(DbContext context)
        {
            try
            {
                // Check if the bridge table exists
                var exists = await context.Database.ExecuteSqlRawAsync(@"
                    IF OBJECT_ID('LauncherCategoryBridge', 'U') IS NOT NULL
                        SELECT 1
                    ELSE
                        SELECT 0");
                
                if (exists == 0)
                {
                    // Create table manually if needed
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE LauncherCategoryBridge (
                            ID INT IDENTITY(1,1) PRIMARY KEY,
                            LauncherID INT NOT NULL,
                            CategoryID INT NOT NULL,
                            CONSTRAINT FK_LauncherCategoryBridge_Launcher FOREIGN KEY (LauncherID) 
                                REFERENCES Launcher(ID),
                            CONSTRAINT FK_LauncherCategoryBridge_Category FOREIGN KEY (CategoryID) 
                                REFERENCES Categories(ID)
                        );
                        
                        CREATE INDEX IX_LauncherCategoryBridge_CategoryID ON LauncherCategoryBridge (CategoryID);
                        CREATE INDEX IX_LauncherCategoryBridge_LauncherID ON LauncherCategoryBridge (LauncherID);");
                    
                    return true;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking bridge table: {ex.Message}");
                return false;
            }
        }
    }
}
