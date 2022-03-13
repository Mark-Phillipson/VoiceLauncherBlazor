using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
	public class LauncherMultipleLauncherBridgeDataService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public LauncherMultipleLauncherBridgeDataService(IDbContextFactory<ApplicationDbContext> context)
		{
			_contextFactory = context;
		}
		public async Task<List<LauncherMultipleLauncherBridge>> GetLMLBsAsync(int? launcherId,int? multipleLauncherId)
		{
			using var context = _contextFactory.CreateDbContext();
			IQueryable<LauncherMultipleLauncherBridge> bridges = null;
			try
			{
				bridges = context.LauncherMultipleLauncherBridge.Include(i => i.Launcher)
					.Include(i => i.MultipleLauncher)
					.OrderBy(v => v.Launcher.Name);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return await bridges.Take(200).ToListAsync();
		}
		public async Task<LauncherMultipleLauncherBridge> GetLauncherMultipleLauncherBridgeAsync(int launcherMultipleLauncherBridgeId)
		{
			using var context = _contextFactory.CreateDbContext();
			LauncherMultipleLauncherBridge bridge = await context.LauncherMultipleLauncherBridge.Include(i => i.Launcher).Include(i => i.MultipleLauncher)
				.Where(v => v.Id == launcherMultipleLauncherBridgeId).FirstOrDefaultAsync();
			return bridge;
		}
		public async Task<string> SaveLauncherMultipleLauncherBridge(LauncherMultipleLauncherBridge bridge)
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
			try
			{
				await context.SaveChangesAsync();
				return $"Launcher/Multiple Launcher Link Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}
		public async Task<string> DeleteLauncherMultipleLauncherBridge(int launcherMultipleLauncherBridgeId)
		{
			using var context = _contextFactory.CreateDbContext();
			var bridge = await context.LauncherMultipleLauncherBridge.Where(v => v.Id == launcherMultipleLauncherBridgeId).FirstOrDefaultAsync();
			var result = $"Delete Launcher Multiple/Launcher Link Failed {DateTime.UtcNow:h:mm:ss tt zz}";
			if (bridge != null)
			{
				context.LauncherMultipleLauncherBridge.Remove(bridge);
				await context.SaveChangesAsync();
				result = $"Launcher Multiple/Launcher Link Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			return result;
		}

	}
}
