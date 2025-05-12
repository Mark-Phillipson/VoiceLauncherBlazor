using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.DataMigration
{
    public class LauncherCategoryMigrator
    {
        private readonly DbContext _context;

        public LauncherCategoryMigrator(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MigrationResult> MigrateCategoriesToBridgeAsync()
        {
            var result = new MigrationResult();
            
            try
            {
                // Get all launchers with valid category IDs
                var launchers = await _context.Set<Launcher>()
                    .Where(l => l.CategoryId > 0)
                    .ToListAsync();
                
                result.TotalLaunchersFound = launchers.Count;
                
                foreach (var launcher in launchers)
                {
                    // Check if this relationship already exists
                    var existingBridge = await _context.Set<LauncherCategoryBridge>()
                        .FirstOrDefaultAsync(b => b.LauncherId == launcher.Id && b.CategoryId == launcher.CategoryId);
                    
                    if (existingBridge == null)
                    {
                        // Create new bridge record
                        var bridge = new LauncherCategoryBridge
                        {
                            LauncherId = launcher.Id,
                            CategoryId = launcher.CategoryId
                        };
                        
                        _context.Add(bridge);
                        result.NewRelationshipsCreated++;
                    }
                    else
                    {
                        result.ExistingRelationshipsFound++;
                    }
                }
                
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
            }
            
            return result;
        }

        public class MigrationResult
        {
            public bool Success { get; set; }
            public int TotalLaunchersFound { get; set; }
            public int NewRelationshipsCreated { get; set; }
            public int ExistingRelationshipsFound { get; set; }
            public string? ErrorMessage { get; set; }
            public Exception? Exception { get; set; }
        }
    }
}
