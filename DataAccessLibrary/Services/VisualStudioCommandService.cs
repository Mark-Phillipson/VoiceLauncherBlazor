using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DataAccessLibrary.Services
{
	public class VisualStudioCommandService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public VisualStudioCommandService(IDbContextFactory<ApplicationDbContext> context)
		{
			_contextFactory = context;
		}
		public async Task<List<VisualStudioCommand>> GetVisualStudioCommandsAsync( int take= 300)
		{
			using var context = _contextFactory.CreateDbContext();
			IQueryable<VisualStudioCommand> visualStudioCommands = null;
			try
			{
				visualStudioCommands = context.VisualStudioCommands;
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
			return await visualStudioCommands.Take(take).ToListAsync();
		}
		public async Task<List<VisualStudioCommand>> GetVisualStudioCommandsAsync(string caption)
		{
			using var context = _contextFactory.CreateDbContext();
			IQueryable<VisualStudioCommand> visualStudioCommands = null;
			try
			{
				visualStudioCommands = context.VisualStudioCommands.Where(v => v.Caption.ToLower().Contains(caption.ToLower()));
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
			return await visualStudioCommands.ToListAsync();
		}
		public async Task<VisualStudioCommand> GetVisualStudioCommandAsync(int visualStudioCommandId)
		{
			using var context = _contextFactory.CreateDbContext();
			VisualStudioCommand visualStudioCommand = await context.VisualStudioCommands.Where(v => v.Id == visualStudioCommandId).FirstOrDefaultAsync();
			return visualStudioCommand;
		}
		public async Task<string> SaveVisualStudioCommand(VisualStudioCommand visualStudioCommand)
		{
			using var context = _contextFactory.CreateDbContext();
			if (visualStudioCommand.Id > 0)
			{
				context.VisualStudioCommands.Update(visualStudioCommand);
			}
			else
			{
				context.VisualStudioCommands.Add(visualStudioCommand);
			}
			try
			{
				await context.SaveChangesAsync();
				return $"VisualStudioCommand Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}
		public async Task<string> DeleteVisualStudioCommand(int visualStudioCommandId)
		{
			using var context = _contextFactory.CreateDbContext();
			var visualStudioCommand = await context.VisualStudioCommands.Where(v => v.Id == visualStudioCommandId).FirstOrDefaultAsync();
			var result = $"Delete VisualStudioCommand Failed {DateTime.UtcNow:h:mm:ss tt zz}";
			if (visualStudioCommand != null)
			{
				context.VisualStudioCommands.Remove(visualStudioCommand);
				await context.SaveChangesAsync();
				result = $"VisualStudioCommand Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			return result;
		}
		public async Task<List<VisualStudioCommand>> SaveAllVisualStudioCommands(List<VisualStudioCommand> visualStudioCommands)
		{
			foreach (var visualStudioCommand in visualStudioCommands)
			{
				await SaveVisualStudioCommand(visualStudioCommand);
			}
			return visualStudioCommands;
		}

	}
}
