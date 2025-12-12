using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DataAccessLibrary.Models
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Create a minimal configuration for design-time
            var configurationBuilder = new ConfigurationBuilder();
            
            // Try to find appsettings.json in various locations
            var basePaths = new[]
            {
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), ".."),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "VoiceLauncherBlazor"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "VoiceAdmin")
            };

            foreach (var basePath in basePaths)
            {
                var appsettingsPath = Path.Combine(basePath, "appsettings.json");
                if (File.Exists(appsettingsPath))
                {
                    configurationBuilder.SetBasePath(basePath)
                        .AddJsonFile("appsettings.json", optional: true);
                    break;
                }
            }

            var configuration = configurationBuilder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Try to get connection string from configuration, fallback to hardcoded
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=Localhost;Initial Catalog=VoiceLauncher;Integrated Security=True;Connect Timeout=120;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options, configuration);
        }
    }
}
