using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
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
		public LauncherService(IDbContextFactory<ApplicationDbContext> context)
		{
			_contextFactory = context;
		}
		public async Task<List<MultipleLauncher>> GetMultipleLaunchersAsync(string? SearchTerm = null)
		{
			using var context = _contextFactory.CreateDbContext();
			IQueryable<MultipleLauncher> multipleLaunchers = new List<MultipleLauncher>().AsQueryable();
			multipleLaunchers = context.MultipleLauncher.Include(i => i.LaunchersMultipleLauncherBridges).ThenInclude(t => t.Launcher).OrderBy(v => v.Description);
			if (SearchTerm != null)
			{
				multipleLaunchers = multipleLaunchers.Where(v => v.Description != null && v.Description.Contains(SearchTerm));
			}
			return await multipleLaunchers.ToListAsync();
		}

		public async Task<List<Launcher>> GetLaunchersAsync(string? searchTerm = null, string? sortColumn = null, string? sortType = null, int? categoryIdFilter = null, int maximumRows = 400)
		{
			using var context = _contextFactory.CreateDbContext();
			IQueryable<Launcher> launchers = new List<Launcher>().AsQueryable();
			try
			{
				launchers = context.Launcher.Include(i => i.Category).Include(i => i.Computer).OrderBy(v => v.Name);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				return new List<Launcher>();
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
		public async Task<Launcher?> GetLauncherAsync(int launcherId)
		{
			using var context = _contextFactory.CreateDbContext();
			Launcher? launcher = await context.Launcher.Include(i => i.Category).Include(n => n.Computer).Where(v => v.Id == launcherId).FirstOrDefaultAsync();
			return launcher;
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
			var launcher = await context.Launcher.Where(v => v.Id == launcherId).FirstOrDefaultAsync();
			var result = $"Delete Launcher Failed {DateTime.UtcNow:h:mm:ss tt zz}";
			if (launcher != null)
			{
				context.Launcher.Remove(launcher);
				await context.SaveChangesAsync();
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
	}
}
