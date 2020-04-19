using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Models;

namespace VoiceLauncherBlazor.Data
{
    public class LauncherService
    {
        private readonly ApplicationDbContext _context;
        public LauncherService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<MultipleLauncher>> GetMultipleLaunchersAsync(string SearchTerm = null)
        {
            IQueryable<MultipleLauncher> multipleLaunchers = null;
            multipleLaunchers = _context.MultipleLauncher.Include(i => i.LaunchersMultipleLauncherBridges).ThenInclude(t => t.Launcher).OrderBy(v => v.Description);
            if (SearchTerm != null)
            {
                multipleLaunchers = multipleLaunchers.Where(v => v.Description.Contains(SearchTerm));
            }
            return await multipleLaunchers.ToListAsync();
        }

        public async Task<List<Launcher>> GetLaunchersAsync(string searchTerm = null, string sortColumn = null, string sortType = null, int? categoryIdFilter = null, int maximumRows = 200)
        {
            IQueryable<Launcher> launchers = null;
            try
            {
                launchers = _context.Launcher.Include(i => i.Category).Include(i => i.Computer).OrderBy(v => v.Name);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            if (searchTerm != null && searchTerm.Length > 0)
            {
                launchers = launchers.Where(v => v.Name.Contains(searchTerm) || v.CommandLine.Contains(searchTerm));
            }
            if (sortType != null && sortColumn != null)
            {
                if (sortColumn == "CategoryName" && sortType == "Ascending")
                {
                    launchers = launchers.OrderBy(v => v.Category.CategoryName);
                }
                else if (sortColumn == "CategoryName" && sortType == "Descending")
                {
                    launchers = launchers.OrderByDescending(v => v.Category.CategoryName);
                }
            }
            if (categoryIdFilter != null)
            {
                launchers = launchers.Where(v => v.CategoryId == categoryIdFilter);
            }
            return await launchers.Take(maximumRows).ToListAsync();
        }
        public async Task<Launcher> GetLauncherAsync(int launcherId)
        {
            Launcher launcher = await _context.Launcher.Include(i => i.Category).Include(n => n.Computer).Where(v => v.Id == launcherId).FirstOrDefaultAsync();
            return launcher;
        }
        public async Task<string> SaveBridge(LauncherMultipleLauncherBridge bridge)
        {
            if (bridge.Id > 0)
            {
                _context.LauncherMultipleLauncherBridge.Update(bridge);
            }
            else
            {
                _context.LauncherMultipleLauncherBridge.Add(bridge);
            }
            await _context.SaveChangesAsync();
            return "successfully saved!";
        }
        public async Task<string> SaveMultipleLauncher(MultipleLauncher multipleLauncher)
        {
            if (multipleLauncher.Id > 0)
            {
                _context.MultipleLauncher.Update(multipleLauncher);
            }
            else
            {
                _context.MultipleLauncher.Add(multipleLauncher);
            }
            await _context.SaveChangesAsync();
            return "Successfully Saved Multiple Launcher!";
        }

        private async Task RemoveBridges(MultipleLauncher multipleLauncher)
        {
            foreach (var bridge in _context.LauncherMultipleLauncherBridge.Where(v => v.MultipleLauncherId == multipleLauncher.Id))
            {
                _context.LauncherMultipleLauncherBridge.Remove(bridge);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<string> SaveLauncher(Launcher launcher)
        {
            if (launcher.Id > 0)
            {
                _context.Launcher.Update(launcher);
            }
            else
            {
                _context.Launcher.Add(launcher);
            }
            try
            {
                await _context.SaveChangesAsync();
                return $"Launcher Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        public async Task<string> DeleteLauncher(int launcherId)
        {
            var launcher = await _context.Launcher.Where(v => v.Id == launcherId).FirstOrDefaultAsync();
            var result = $"Delete Launcher Failed {DateTime.UtcNow:h:mm:ss tt zz}";
            if (launcher != null)
            {
                _context.Launcher.Remove(launcher);
                await _context.SaveChangesAsync();
                result = $"Launcher Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            return result;
        }
        public async Task<List<MultipleLauncher>> SaveAllMultipleLauncher(List<MultipleLauncher> multipleLaunchers)
        {
            foreach (var multipleLauncher in multipleLaunchers)
            {
                await SaveMultipleLauncher(multipleLauncher);
            }
            return multipleLaunchers;
        }
        public async Task<string> DeleteMultipleLauncher(int multipleLauncherId)
        {
            var multipleLauncher = await _context.MultipleLauncher.Where(v => v.Id == multipleLauncherId).FirstOrDefaultAsync();
            await RemoveBridges(multipleLauncher);
            _context.MultipleLauncher.Remove(multipleLauncher);
            await _context.SaveChangesAsync();
            return "Multiple Launcher has been deleted successfully!";
        }
        public async Task<string> DeleteBridge(LauncherMultipleLauncherBridge bridge)
        {
            _context.LauncherMultipleLauncherBridge.Remove(bridge);
            await _context.SaveChangesAsync();
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

    }
}
