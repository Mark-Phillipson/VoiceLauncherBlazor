using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
	public class CustomIntellisenseService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public CustomIntellisenseService(IDbContextFactory<ApplicationDbContext> context)
		{
			_contextFactory = context;
		}
		public async Task<List<CustomIntelliSense>> GetCustomIntelliSensesAsync(string searchTerm = null, string sortColumn = null, string sortType = null, int? categoryIdFilter = null, int? languageIdFilter = null, int maximumRows = 200)
		{
			using var context = _contextFactory.CreateDbContext();
			IQueryable<CustomIntelliSense> intellisenses = null;
			try
			{
				intellisenses = context.CustomIntelliSense.Include(i => i.Category).Include(i => i.Language).OrderBy(v => v.Category);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
			if (Environment.MachineName != "SURFACEPRO")
			{
				intellisenses = intellisenses.Where(v => v.Category.Sensitive == false);
			}
			if (searchTerm != null && searchTerm.Length > 0)
			{
				intellisenses = intellisenses.Where(v => v.DisplayValue.Contains(searchTerm) || v.SendKeysValue.Contains(searchTerm));
			}
			if (sortType != null && sortColumn != null)
			{
				if (sortColumn == "LanguageName" && sortType == "Ascending")
				{
					intellisenses = intellisenses.OrderBy(v => v.Language.LanguageName);
				}
				else if (sortColumn == "LanguageName" && sortType == "Descending")
				{
					intellisenses = intellisenses.OrderByDescending(v => v.Language.LanguageName);
				}
				else if (sortColumn == "DisplayValue" && sortType == "Ascending")
				{
					intellisenses = intellisenses.OrderBy(v => v.DisplayValue);
				}
				else if (sortColumn == "DisplayValue" && sortType == "Descending")
				{
					intellisenses = intellisenses.OrderByDescending(v => v.DisplayValue);
				}
			}
			else
			{
				intellisenses = intellisenses.OrderBy(v => v.Language.LanguageName).ThenBy(t => t.Category.CategoryName).ThenBy(b => b.DisplayValue);
			}
			if (languageIdFilter != null)
			{
				intellisenses = intellisenses.Where(v => v.LanguageId == languageIdFilter);
			}
			if (categoryIdFilter != null)
			{
				intellisenses = intellisenses.Where(v => v.CategoryId == categoryIdFilter);
			}

			return await intellisenses.Take(maximumRows).ToListAsync();
		}
		public async Task<CustomIntelliSense> GetCustomIntelliSenseAsync(int intellisenseId)
		{
			using var context = _contextFactory.CreateDbContext();
			CustomIntelliSense intellisense = await context.CustomIntelliSense.Include(i => i.Language).Include(i => i.Category).Where(v => v.Id == intellisenseId).FirstOrDefaultAsync();
			return intellisense;
		}
		public async Task<string> SaveCustomIntelliSense(CustomIntelliSense intellisense)
		{
			using var context = _contextFactory.CreateDbContext();
			if (intellisense.Id > 0)
			{
				var existingIntellisense =  await context.CustomIntelliSense.FirstOrDefaultAsync(c => c.Id == intellisense.Id);
				if (existingIntellisense!= null )
				{
					existingIntellisense.LanguageId = intellisense.LanguageId;
					existingIntellisense.CategoryId= intellisense.CategoryId;
					existingIntellisense.CommandType= intellisense.CommandType;
					existingIntellisense.Remarks= intellisense.Remarks;
					existingIntellisense.ComputerId= intellisense.ComputerId;
					existingIntellisense.DeliveryType= intellisense.DeliveryType;
					existingIntellisense.DisplayValue= intellisense.DisplayValue;
					existingIntellisense.SendKeysValue= intellisense.SendKeysValue;
				}
			}
			else
			{
				context.CustomIntelliSense.Add(intellisense);
			}
			try
			{
				await context.SaveChangesAsync();
				return $"Custom IntelliSense Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}
		public async Task<string> DeleteCustomIntelliSense(int customIntellisenseId)
		{
			using var context = _contextFactory.CreateDbContext();
			var intellisense = await context.CustomIntelliSense.Where(v => v.Id == customIntellisenseId).FirstOrDefaultAsync();
			var result = $"Delete Custom IntelliSense Failed {DateTime.UtcNow:h:mm:ss tt zz}";
			if (intellisense != null)
			{
				context.CustomIntelliSense.Remove(intellisense);
				try
				{
					await context.SaveChangesAsync();
				}
				catch (Exception exception)
				{
					return $"{result} {exception.Message}";
				}
				result = $"Custom IntelliSense Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			return result;
		}
		public async Task<List<CustomIntelliSense>> SaveAllCustomIntelliSenses(List<CustomIntelliSense> intellisenses)
		{
			foreach (var intellisense in intellisenses)
			{
				await SaveCustomIntelliSense(intellisense);
			}
			return intellisenses;
		}

	}
}
