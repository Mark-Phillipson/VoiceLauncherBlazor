using AutoMapper;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories;

	public class LauncherRepository : ILauncherRepository
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		private readonly IMapper _mapper;

		public LauncherRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
		{
			_contextFactory = contextFactory;
        _mapper = mapper;
		}

    public async Task<IEnumerable<LauncherDTO>> GetAllLaunchersAsync(int CategoryId)
		{
			using var context = _contextFactory.CreateDbContext();
			List<Launcher> launchers;
			IEnumerable<LauncherDTO> launchersDTO;
			if (CategoryId == 0)
			{
				launchers = await context.Launcher
					.OrderBy(v => v.SortOrder)
					.ThenBy(x => x.Name)
					.ToListAsync();
				launchersDTO = _mapper.Map<List<Launcher>, IEnumerable<LauncherDTO>>(launchers);
				return launchersDTO;
			}
			launchers = await context.Launcher
								.Where(v => v.LauncherCategoryBridges.Any(b => b.CategoryId == CategoryId) || v.CategoryId == CategoryId)
								.OrderBy(v => v.SortOrder)
								.ThenBy(x => x.Name)
								.ToListAsync();
			launchersDTO = _mapper.Map<List<Launcher>, IEnumerable<LauncherDTO>>(launchers);
			return launchersDTO;
		}
		public async Task<IEnumerable<LauncherDTO>> SearchLaunchersAsync(string serverSearchTerm)
		{
			using var context = _contextFactory.CreateDbContext();
			var Launchers = await context.Launcher
					.Where(x => x.Name.ToLower().Contains(serverSearchTerm.ToLower()))
					.OrderBy(v => v.Name)
					.Take(500)
					.ToListAsync();
			IEnumerable<LauncherDTO> LaunchersDTO = _mapper.Map<List<Launcher>, IEnumerable<LauncherDTO>>(Launchers);
			return LaunchersDTO;
		}

		public async Task<LauncherDTO?> GetLauncherByIdAsync(int Id)
		{
			using var context = _contextFactory.CreateDbContext();
			var result = await context.Launcher.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Id == Id);
			if (result == null) return null;
			LauncherDTO launcherDTO = _mapper.Map<Launcher, LauncherDTO>(result);
			return launcherDTO;
		}

		public async Task<LauncherDTO?> AddLauncherAsync(LauncherDTO launcherDTO)
		{
			using var context = _contextFactory.CreateDbContext();
			Launcher launcher = _mapper.Map<LauncherDTO, Launcher>(launcherDTO);
			var addedEntity = context.Launcher.Add(launcher);
			try
			{
				await context.SaveChangesAsync();
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				return null;
			}
			LauncherDTO resultDTO = _mapper.Map<Launcher, LauncherDTO>(launcher);
			return resultDTO;
		}

		public async Task<LauncherDTO?> UpdateLauncherAsync(LauncherDTO launcherDTO)
		{
			Launcher launcher = _mapper.Map<LauncherDTO, Launcher>(launcherDTO);
			using (var context = _contextFactory.CreateDbContext())
			{
				var foundLauncher = await context.Launcher.AsNoTracking().FirstOrDefaultAsync(e => e.Id == launcher.Id);

				if (foundLauncher != null)
				{
					var mappedLauncher = _mapper.Map<Launcher>(launcher);
					context.Launcher.Update(mappedLauncher);
					await context.SaveChangesAsync();
					LauncherDTO resultDTO = _mapper.Map<Launcher, LauncherDTO>(mappedLauncher);
					return resultDTO;
				}
			}
			return null;
		}
		public async Task DeleteLauncherAsync(int Id)
		{
			using var context = _contextFactory.CreateDbContext();
			// Remove all related LauncherCategoryBridge records
			var relatedBridges = context.LauncherCategoryBridges.Where(b => b.LauncherId == Id);
			if (relatedBridges.Any())
			{
				context.LauncherCategoryBridges.RemoveRange(relatedBridges);
			}
			var foundLauncher = context.Launcher.FirstOrDefault(e => e.Id == Id);
			if (foundLauncher == null)
			{
				return;
			}
			context.Launcher.Remove(foundLauncher);
			await context.SaveChangesAsync();
		}

		public async Task<IEnumerable<LauncherDTO>> GetFavoriteLaunchersAsync()
		{
			using var context = _contextFactory.CreateDbContext();
			var Launchers = await context.Launcher
					.Where(v => v.Favourite == true)
					.OrderBy(v => v.SortOrder)
					.ThenBy(x => x.Name)
					.ToListAsync();
			IEnumerable<LauncherDTO> LaunchersDTO = _mapper.Map<List<Launcher>, IEnumerable<LauncherDTO>>(Launchers);
			return LaunchersDTO;

		}

    public async Task UpdateLauncherCategoriesAsync(int launcherId, HashSet<int> selectedCategoryIds)
    {
        using var context = _contextFactory.CreateDbContext();
        
        // Get existing category associations for this launcher
        var existingAssociations = await context.LauncherCategoryBridges
            .Where(lcb => lcb.LauncherId == launcherId)
            .ToListAsync();
        
        // Remove associations that aren't in the selectedCategoryIds
        var associationsToRemove = existingAssociations
            .Where(a => !selectedCategoryIds.Contains(a.CategoryId))
            .ToList();
            
        if (associationsToRemove.Any())
        {
            context.LauncherCategoryBridges.RemoveRange(associationsToRemove);
        }
        
        // Add new associations that don't already exist
        var existingCategoryIds = existingAssociations.Select(a => a.CategoryId).ToHashSet();
        var categoriesToAdd = selectedCategoryIds
            .Where(id => !existingCategoryIds.Contains(id))
            .Select(categoryId => new LauncherCategoryBridge 
            { 
                LauncherId = launcherId, 
                CategoryId = categoryId 
            })
            .ToList();
            
        if (categoriesToAdd.Any())
        {
            await context.LauncherCategoryBridges.AddRangeAsync(categoriesToAdd);
        }
        
        // Save changes to the database
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<int>> GetCategoryIdsForLauncherAsync(int launcherId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        // Query the bridge table to get all category IDs associated with this launcher
        var categoryIds = await context.LauncherCategoryBridges
            .Where(lcb => lcb.LauncherId == launcherId)
            .Select(lcb => lcb.CategoryId)
            .ToListAsync();
            
        return categoryIds;
    }
}