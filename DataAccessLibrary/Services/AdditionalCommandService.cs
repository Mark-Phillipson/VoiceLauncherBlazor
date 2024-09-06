using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
	public class AdditionalCommandService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public AdditionalCommandService(IDbContextFactory<ApplicationDbContext> context)
		{
			_contextFactory = context;
		}
		public async Task<List<AdditionalCommand>> GetAdditionalCommandsAsync(int customIntellisenseId)
		{
			using var context = _contextFactory.CreateDbContext();
			IQueryable<AdditionalCommand> additionalCommands = new List<AdditionalCommand>().AsQueryable();
			try
			{
				additionalCommands = context.AdditionalCommands.Include(i => i.CustomIntelliSense)
					.Where(v => v.CustomIntelliSenseId == customIntellisenseId);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
			if (additionalCommands == null)
			{
				return new List<AdditionalCommand>();
			}
			return await additionalCommands.ToListAsync();
		}
		public async Task<AdditionalCommand?> GetAdditionalCommandAsync(int additionalCommandId)
		{
			using var context = _contextFactory.CreateDbContext();
			AdditionalCommand? additionalCommand = await context.AdditionalCommands.Include("CustomIntelliSense").Where(v => v.Id == additionalCommandId).FirstOrDefaultAsync();
			return additionalCommand;
		}
		public async Task<string> SaveAdditionalCommand(AdditionalCommand additionalCommand)
		{
			using var context = _contextFactory.CreateDbContext();
			if (additionalCommand.Id > 0)
			{
				context.AdditionalCommands.Update(additionalCommand);
			}
			else
			{
				context.AdditionalCommands.Add(additionalCommand);
			}
			try
			{
				await context.SaveChangesAsync();
				return $"AdditionalCommand Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}
		public async Task<string> DeleteAdditionalCommand(int additionalCommandId)
		{
			using var context = _contextFactory.CreateDbContext();
			var additionalCommand = await context.AdditionalCommands.Where(v => v.Id == additionalCommandId).FirstOrDefaultAsync();
			var result = $"Delete AdditionalCommand Failed {DateTime.UtcNow:h:mm:ss tt zz}";
			if (additionalCommand != null)
			{
				context.AdditionalCommands.Remove(additionalCommand);
				await context.SaveChangesAsync();
				result = $"AdditionalCommand Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			return result;
		}
		public async Task<List<AdditionalCommand>> SaveAllAdditionalCommands(List<AdditionalCommand> additionalCommands)
		{
			foreach (var additionalCommand in additionalCommands)
			{
				await SaveAdditionalCommand(additionalCommand);
			}
			return additionalCommands;
		}

	}
}
