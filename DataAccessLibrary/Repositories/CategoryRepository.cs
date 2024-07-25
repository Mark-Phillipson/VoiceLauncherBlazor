
using AutoMapper;
using DataAccessLibrary.DTO;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;

using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore;

namespace VoiceLauncher.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		private readonly IMapper _mapper;

		public CategoryRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
		{
			_contextFactory = contextFactory;
			this._mapper = mapper;
		}
		public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesByTypeAsync(string categoryType)
		{
			using var context = _contextFactory.CreateDbContext();
			var Categories = await context.Categories
				.Where(v => v.CategoryType.ToLower() == categoryType.ToLower())
				.OrderBy(v => v.CategoryName)
				.ToListAsync();
			IEnumerable<CategoryDTO> CategoriesDTO = _mapper.Map<List<Category>, IEnumerable<CategoryDTO>>(Categories);
			return CategoriesDTO;
		}
		public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync(int maxRows = 400, string categoryType = "Launch Applications", int languageId = 0)
		{
			using var context = _contextFactory.CreateDbContext();
			List<Category> Categories = null;
			if (languageId > 0)
			{
				Categories = await context.Categories
				 .Include(i => i.CustomIntelliSense)
				 .Where(v => v.CategoryType == categoryType && v.CustomIntelliSense.Count() > 0 && v.CustomIntelliSense.Any(c => c.LanguageId == languageId))
				 .OrderBy(v => v.CategoryName)
				 .Take(maxRows)
				 .ToListAsync();

			}
			else if (categoryType == "IntelliSense Command")
			{
				Categories = await context.Categories
				 .Include(i => i.CustomIntelliSense)
				 .Where(v => v.CategoryType == categoryType)
				 .OrderBy(v => v.CategoryName)
				 .Take(maxRows)
				 .ToListAsync();
			}
			else
			{
				Categories = await context.Categories
					 .Include(i => i.Launchers)
					 .Where(v => v.CategoryType == categoryType)
					 .OrderBy(v => v.CategoryName)
					 .Take(maxRows)
					 .ToListAsync();
			}
			IEnumerable<CategoryDTO> CategoriesDTO = _mapper.Map<List<Category>, IEnumerable<CategoryDTO>>(Categories);
			if (categoryType == "IntelliSense Command")

			// Add the missing using directive above

			{
				foreach (var category in CategoriesDTO)
				{
					var tableCategory = Categories
						.Where(v => v.Id == category.Id)
						.FirstOrDefault();
					category.CountOfCustomIntellisense = tableCategory.CustomIntelliSense.Count(c => c.LanguageId == languageId);
				}
			}
			else
			{
				foreach (var category in CategoriesDTO)
				{
					var tableCategory = Categories.Where(v => v.Id == category.Id).FirstOrDefault();
					category.CountOfLaunchers = tableCategory.Launchers.Count;
				}

			}
			return CategoriesDTO;
		}
		public async Task<IEnumerable<CategoryDTO>> SearchCategoriesAsync(string serverSearchTerm)
		{
			using var context = _contextFactory.CreateDbContext();
			var Categories = await context.Categories
				 .Where(x => x.CategoryName.ToLower().Contains(serverSearchTerm.ToLower()))
				 .OrderBy(x => x.CategoryName)
				 .Take(1000)
				 .ToListAsync();
			IEnumerable<CategoryDTO> CategoriesDTO = _mapper.Map<List<Category>, IEnumerable<CategoryDTO>>(Categories);
			return CategoriesDTO;
		}

		public async Task<CategoryDTO> GetCategoryByIdAsync(int Id)
		{
			using var context = _contextFactory.CreateDbContext();
			var result = await context.Categories.AsNoTracking()
			  .FirstOrDefaultAsync(c => c.Id == Id);
			if (result == null) return null;
			CategoryDTO categoryDTO = _mapper.Map<Category, CategoryDTO>(result);
			return categoryDTO;
		}

		public async Task<string> AddCategoryAsync(CategoryDTO categoryDTO)
		{
			using var context = _contextFactory.CreateDbContext();
			Category category = _mapper.Map<CategoryDTO, Category>(categoryDTO);
			var addedEntity = context.Categories.Add(category);
			try
			{
				await context.SaveChangesAsync();
			}
			catch (Exception exception)
			{
				if (exception.InnerException != null)
				{
					Console.WriteLine(exception.InnerException.Message);
					return exception.InnerException.Message;
				}
				else
				{
					Console.WriteLine(exception.Message);
					return exception.Message;
				}
			}
			CategoryDTO resultDTO = _mapper.Map<Category, CategoryDTO>(category);
			return $"{category.CategoryName} added successfully";
		}

		public async Task<CategoryDTO> UpdateCategoryAsync(CategoryDTO categoryDTO)
		{
			Category category = _mapper.Map<CategoryDTO, Category>(categoryDTO);
			using (var context = _contextFactory.CreateDbContext())
			{
				var foundCategory = await context.Categories.AsNoTracking().FirstOrDefaultAsync(e => e.Id == category.Id);

				if (foundCategory != null)
				{
					var mappedCategory = _mapper.Map<Category>(category);
					context.Categories.Update(mappedCategory);
					await context.SaveChangesAsync();
					CategoryDTO resultDTO = _mapper.Map<Category, CategoryDTO>(mappedCategory);
					return resultDTO;
				}
			}
			return null;
		}
		public async Task DeleteCategoryAsync(int Id)
		{
			using var context = _contextFactory.CreateDbContext();
			var foundCategory = context.Categories.FirstOrDefault(e => e.Id == Id);
			if (foundCategory == null)
			{
				return;
			}
			context.Categories.Remove(foundCategory);
			await context.SaveChangesAsync();
		}
	}
}