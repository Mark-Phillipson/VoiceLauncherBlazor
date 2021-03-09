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
		private readonly ApplicationDbContext _context;
		public AdditionalCommandService(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task<List<AdditionalCommand>> GetAdditionalCommandsAsync(int customIntellisenseId)
		{
			IQueryable<AdditionalCommand> additionalCommands = null;
			try
			{
				additionalCommands = _context.AdditionalCommands.Include(i => i.CustomIntelliSense)
					.Where(v => v.CustomIntelliSenseId==customIntellisenseId);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
			return await additionalCommands.ToListAsync();
		}
	public async Task<AdditionalCommand> GetAdditionalCommandAsync(int additionalCommandId)
	{
		AdditionalCommand additionalCommand = await _context.AdditionalCommands.Include("CustomIntelliSense").Where(v => v.Id == additionalCommandId).FirstOrDefaultAsync();
		return additionalCommand;
	}
	public async Task<string> SaveAdditionalCommand(AdditionalCommand additionalCommand)
	{
		if (additionalCommand.Id > 0)
		{
			_context.AdditionalCommands.Update(additionalCommand);
		}
		else
		{
			_context.AdditionalCommands.Add(additionalCommand);
		}
		try
		{
			await _context.SaveChangesAsync();
			return $"AdditionalCommand Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
		}
		catch (Exception exception)
		{
			return exception.Message;
		}
	}
	public async Task<string> DeleteAdditionalCommand(int additionalCommandId)
	{
		var additionalCommand = await _context.AdditionalCommands.Where(v => v.Id == additionalCommandId).FirstOrDefaultAsync();
		var result = $"Delete AdditionalCommand Failed {DateTime.UtcNow:h:mm:ss tt zz}";
		if (additionalCommand != null)
		{
			_context.AdditionalCommands.Remove(additionalCommand);
			await _context.SaveChangesAsync();
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
