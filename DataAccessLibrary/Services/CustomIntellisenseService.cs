using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartComponents.LocalEmbeddings;

namespace DataAccessLibrary.Services
{
	public class CustomIntellisenseService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public CustomIntellisenseService(IDbContextFactory<ApplicationDbContext> context)
		{
			_contextFactory = context;
		}
		public async Task<List<CustomIntelliSense>> GetCustomIntelliSensesAsync(string? searchTerm = null, string? sortColumn = null, string? sortType = null, int? categoryIdFilter = null, int? languageIdFilter = null, int maximumRows = 2000, string? languageFilter = null, string? categoryFilter = null, bool useSemanticMatching = false)
		{
			using var context = _contextFactory.CreateDbContext();
			IQueryable<CustomIntelliSense> intellisenses = new List<CustomIntelliSense>().AsQueryable();
			try
			{
				intellisenses = context.CustomIntelliSenses.Include(i => i.Category).AsSingleQuery().Include(i => i.Language).AsSingleQuery().OrderBy(v => v.Category);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
			if (intellisenses == null)
			{
				return new List<CustomIntelliSense>();
			}
			intellisenses = intellisenses.Where(v => v.Language != null && v.Language.Active);
			if (Environment.MachineName != "J40L4V3")
			{
				intellisenses = intellisenses.Where(v => v.Category != null && v.Category.Sensitive == false);
			}
			if (searchTerm != null && searchTerm.Length > 0 && useSemanticMatching == false)
			{
				var searchTerms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				intellisenses = intellisenses.Where(v => searchTerms.All(term => v.DisplayValue != null && v.DisplayValue.Contains(term) || v.SendKeysValue != null && v.SendKeysValue.Contains(term)));
			}
			if (!string.IsNullOrWhiteSpace(languageFilter))
			{
				intellisenses = intellisenses.Where(v => v.Language != null && v.Language.LanguageName.Contains(languageFilter));
			}
			if (!string.IsNullOrWhiteSpace(categoryFilter))
			{
				intellisenses = intellisenses.Where(v => v.Category != null && v.Category.CategoryName.Contains(categoryFilter));
			}
			if (sortType != null && sortColumn != null)
			{
				if (sortColumn == "LanguageName" && sortType == "Ascending")
				{
					intellisenses = intellisenses.OrderBy(v => v.Language!.LanguageName);
				}
				else if (sortColumn == "LanguageName" && sortType == "Descending")
				{
					intellisenses = intellisenses.OrderByDescending(v => v.Language!.LanguageName);
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
				intellisenses = intellisenses.OrderBy(v => v.Language!.LanguageName).ThenBy(t => t.Category!.CategoryName).ThenBy(b => b.DisplayValue);
			}
			if (languageIdFilter != null && languageIdFilter != 0)
			{
				intellisenses = intellisenses.Where(v => v.LanguageId == languageIdFilter);
			}
			if (categoryIdFilter != null && categoryIdFilter != 0)
			{
				intellisenses = intellisenses.Where(v => v.CategoryId == categoryIdFilter);
			}
			if (useSemanticMatching)
			{
				IQueryable<CustomIntelliSense> snippets = new List<CustomIntelliSense>().AsQueryable();
				snippets = intellisenses;
				var result = snippets.Where(f => f.DisplayValue != null).Select(v => v.DisplayValue).Distinct().ToList();
				if (result == null)
				{
					return new List<CustomIntelliSense>();
				}
				List<string> displayValues = result!;
				using var localCommandEmbedder = new LocalEmbedder();
				if (displayValues == null || displayValues.Count == 0)
				{
					return new List<CustomIntelliSense>();
				}
				IList<(string Item, EmbeddingF32 Embedding)> matchedResults;
				matchedResults = localCommandEmbedder.EmbedRange(
					items: displayValues.ToList());
				SimilarityScore<string>[] results = LocalEmbedder.FindClosestWithScore(localCommandEmbedder.Embed(searchTerm ?? ""), matchedResults, maxResults: 20);
				if (results.Length > 0)
				{
					foreach (var item in results)
					{
						Console.WriteLine($"Item: {item.Item} Score: {item.Similarity}");
					}
					var resultItems = results.Select(r => r.Item).ToList();
					var similarityScores = results.ToDictionary(r => r.Item, r => r.Similarity);
					List<CustomIntelliSense> snippetsList = await snippets.ToListAsync();
					snippetsList = snippetsList.Where(v => resultItems.Contains(v.DisplayValue!))
					.OrderByDescending(v => similarityScores[v.DisplayValue!])
					.ToList();
					List<CustomIntelliSense> methodResult = new List<CustomIntelliSense>();
					try
					{
						methodResult = snippetsList.Take(maximumRows).ToList();
					}
					catch (System.Exception exception)
					{
						System.Console.WriteLine(exception.Message);
					}
					return methodResult;
				}
			}

			return await intellisenses.Take(maximumRows).ToListAsync();
		}
		public async Task<CustomIntelliSense?> GetCustomIntelliSenseAsync(int intellisenseId)
		{
			using var context = _contextFactory.CreateDbContext();
			CustomIntelliSense? intellisense = await context.CustomIntelliSenses.Include(i => i.Language).Include(i => i.Category).Where(v => v.Id == intellisenseId).FirstOrDefaultAsync();
			return intellisense;
		}
		public async Task<string> SaveCustomIntelliSense(CustomIntelliSense intellisense)
		{
			using var context = _contextFactory.CreateDbContext();
			if (intellisense.Id > 0)
			{
				var existingIntellisense = await context.CustomIntelliSenses.FirstOrDefaultAsync(c => c.Id == intellisense.Id);
				if (existingIntellisense != null)
				{
					existingIntellisense.LanguageId = intellisense.LanguageId;
					existingIntellisense.CategoryId = intellisense.CategoryId;
					existingIntellisense.CommandType = intellisense.CommandType;
					existingIntellisense.Remarks = intellisense.Remarks;
					existingIntellisense.ComputerId = intellisense.ComputerId;
					existingIntellisense.DeliveryType = intellisense.DeliveryType;
					existingIntellisense.DisplayValue = intellisense.DisplayValue;
					existingIntellisense.SendKeysValue = intellisense.SendKeysValue;
				}
			}
			else
			{
				context.CustomIntelliSenses.Add(intellisense);
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
			var intellisense = await context.CustomIntelliSenses.Where(v => v.Id == customIntellisenseId).FirstOrDefaultAsync();
			var result = $"Delete Custom IntelliSense Failed {DateTime.UtcNow:h:mm:ss tt zz}";
			if (intellisense != null)
			{
				context.CustomIntelliSenses.Remove(intellisense);
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
