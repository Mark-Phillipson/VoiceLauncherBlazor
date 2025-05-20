using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
    public class LauncherService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMemoryCache _cache;        private const string LAUNCHERS_CACHE_KEY = "launchers";
        private const string MULTIPLE_LAUNCHERS_CACHE_KEY = "multiple_launchers";
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public LauncherService(IDbContextFactory<ApplicationDbContext> context, IMemoryCache cache)
        {
            _contextFactory = context;
            _cache = cache;
        }

        public async Task<List<MultipleLauncher>> GetMultipleLaunchersAsync(string? SearchTerm = null)
        {
            string cacheKey = $"{MULTIPLE_LAUNCHERS_CACHE_KEY}_{SearchTerm}";
            
            List<MultipleLauncher>? multipleLaunchers;
            if (!_cache.TryGetValue(cacheKey, out multipleLaunchers))
            {
                using var context = _contextFactory.CreateDbContext();
                var query = context.MultipleLauncher
                    .Include(i => i.LaunchersMultipleLauncherBridges)
                    .ThenInclude(t => t.Launcher)
                    .OrderBy(v => v.Description)
                    .AsQueryable();

                if (SearchTerm != null)
                {
                    query = query.Where(v => v.Description != null && v.Description.Contains(SearchTerm));
                }

                multipleLaunchers = await query.ToListAsync();
                _cache.Set(cacheKey, multipleLaunchers, _cacheDuration);
            }

            return multipleLaunchers ?? new List<MultipleLauncher>();
        }

        public async Task<List<Launcher>> GetLaunchersAsync(string? searchTerm = null, string? sortColumn = null, string? sortType = null, int? categoryIdFilter = null, int maximumRows = 400)
        {
            string cacheKey = $"{LAUNCHERS_CACHE_KEY}_{searchTerm}_{sortColumn}_{sortType}_{categoryIdFilter}_{maximumRows}";
            
            List<Launcher>? launchers;
            if (!_cache.TryGetValue(cacheKey, out launchers))
            {
                using var context = _contextFactory.CreateDbContext();
                var query = context.Launcher
                    .Include(i => i.Category)
                    .Include(i => i.Computer)
                    .OrderBy(v => v.Name)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(v => v.Name.Contains(searchTerm) || v.CommandLine.Contains(searchTerm));
                }

                if (sortType != null && sortColumn != null)
                {
                    if (sortColumn == "CategoryName")
                    {
                        query = sortType == "Ascending" 
                            ? query.OrderBy(v => v.Category.CategoryName)
                            : query.OrderByDescending(v => v.Category.CategoryName);
                    }
                }

                if (categoryIdFilter.HasValue)
                {
                    query = query.Where(v => v.CategoryId == categoryIdFilter.Value);
                }

                launchers = await query.Take(maximumRows).ToListAsync();
                _cache.Set(cacheKey, launchers, _cacheDuration);
            }

            return launchers ?? new List<Launcher>();
        }

        // Cache individual launcher lookups too
        public async Task<Launcher?> GetLauncherAsync(int launcherId)
        {
            string cacheKey = $"launcher_{launcherId}";
            
            Launcher? launcher;
            if (!_cache.TryGetValue(cacheKey, out launcher))
            {
                using var context = _contextFactory.CreateDbContext();
                launcher = await context.Launcher
                    .Include(i => i.Category)
                    .Include(n => n.Computer)
                    .FirstOrDefaultAsync(v => v.Id == launcherId);

                if (launcher != null)
                {
                    _cache.Set(cacheKey, launcher, _cacheDuration);
                }
            }

            return launcher;
        }        public void InvalidateCache(string pattern = "*")
        {
            if (pattern == "*")
            {
                // Remove all launcher-related cache entries
                var field = typeof(MemoryCache).GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entries = field?.GetValue(_cache) as System.Collections.IDictionary;
                
                if (entries != null)
                {
                    var keys = entries.Keys.Cast<object>()
                                    .Where(k => k.ToString()?.StartsWith(LAUNCHERS_CACHE_KEY) == true ||
                                              k.ToString()?.StartsWith(MULTIPLE_LAUNCHERS_CACHE_KEY) == true ||
                                              k.ToString()?.StartsWith("launcher_") == true)
                                    .ToList();
                    
                    foreach (var key in keys)
                    {
                        _cache.Remove(key);
                    }
                }
            }
            else
            {
                _cache.Remove(pattern);
            }
        }

        public async Task<string> SaveLauncher(Launcher launcher)
        {
            using var context = _contextFactory.CreateDbContext();

            if (launcher.Id > 0)
            {
                var existingLauncher = context.Launcher.FirstOrDefault(l => l.Id == launcher.Id);

                if (existingLauncher != null)
                {
                    existingLauncher.Name = launcher.Name;
                    existingLauncher.CommandLine = launcher.CommandLine;
                    existingLauncher.CategoryId = launcher.CategoryId;
                    existingLauncher.ComputerId = launcher.ComputerId;
                    existingLauncher.Favourite = launcher.Favourite;
                    existingLauncher.Icon = launcher.Icon;
                }
            }
            else
            {
                context.Launcher.Add(launcher);
            }

            try
            {
                await context.SaveChangesAsync();
                // Invalidate cache after successful save
                InvalidateCache();
                return $"Launcher Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public async Task<string> DeleteLauncher(int launcherId)
        {
            using var context = _contextFactory.CreateDbContext();
            var launcher = await context.Launcher.FindAsync(launcherId);
            if (launcher != null)
            {
                context.Launcher.Remove(launcher);
                await context.SaveChangesAsync();
                // Invalidate cache after successful deletion
                InvalidateCache();
                return "Launcher deleted successfully!";
            }
            return "Launcher not found";
        }

        public async Task<string> SaveBridge(LauncherMultipleLauncherBridge bridge)
        {
            using var context = _contextFactory.CreateDbContext();
            if (bridge.Id > 0)
            {
                context.LauncherMultipleLauncherBridge.Update(bridge);
            }
            else
            {
                context.LauncherMultipleLauncherBridge.Add(bridge);
            }
            await context.SaveChangesAsync();
            // Invalidate cache since bridge relationships changed
            InvalidateCache();
            return "successfully saved!";
        }
        public async Task<string> SaveMultipleLauncher(MultipleLauncher multipleLauncher)
        {
            using var context = _contextFactory.CreateDbContext();
            if (multipleLauncher.Id > 0)
            {
                context.MultipleLauncher.Update(multipleLauncher);
            }
            else
            {
                context.MultipleLauncher.Add(multipleLauncher);
            }
            await context.SaveChangesAsync();
            return "Successfully Saved Multiple Launcher!";
        }

        private async Task RemoveBridges(MultipleLauncher multipleLauncher)
        {
            using var context = _contextFactory.CreateDbContext();
            foreach (var bridge in context.LauncherMultipleLauncherBridge.Where(v => v.MultipleLauncherId == multipleLauncher.Id))
            {
                context.LauncherMultipleLauncherBridge.Remove(bridge);
            }
            await context.SaveChangesAsync();
        }

        public async Task<string> DeleteMultipleLauncher(int multipleLauncherId)
        {
            using var context = _contextFactory.CreateDbContext();
            var multipleLauncher = await context.MultipleLauncher.Where(v => v.Id == multipleLauncherId).FirstOrDefaultAsync();

            if (multipleLauncher != null)
            {
                await RemoveBridges(multipleLauncher);
                context.MultipleLauncher.Remove(multipleLauncher);
            }
            await context.SaveChangesAsync();
            return "Multiple Launcher has been deleted successfully!";
        }
        public async Task<string> DeleteBridge(LauncherMultipleLauncherBridge bridge)
        {
            using var context = _contextFactory.CreateDbContext();
            context.LauncherMultipleLauncherBridge.Remove(bridge);
            await context.SaveChangesAsync();
            return "Deleted successfully!";
        }
        public async Task<List<Launcher>> SaveAllLaunchers(List<Launcher> launchers)
        {
            foreach (var launcher in launchers)
            {
                await SaveLauncher(launcher);
            }
            return launchers;
        }
        public async Task<List<MultipleLauncher>> SaveAllMultipleLauncher(List<MultipleLauncher> multipleLaunchers)
        {
            foreach (var multipleLauncher in multipleLaunchers)
            {
                await SaveMultipleLauncher(multipleLauncher);
            }
            return multipleLaunchers;
        }
    }
}
