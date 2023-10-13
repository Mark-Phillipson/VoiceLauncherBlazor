using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
	public class CategoryService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public CategoryService(IDbContextFactory<ApplicationDbContext> context)
		{
			this._contextFactory = context;
		}
		public async Task<List<Category>> GetCategoriesByTypeAsync(string categoryType)
		{
			using var context = _contextFactory.CreateDbContext();
			List<Category> categories = await context.Categories.Where(v => v.CategoryType == categoryType).OrderBy(v => v.CategoryName).ToListAsync();
			return categories;
		}
		public async Task<List<Category>> GetCategoriesAsync(string searchTerm = null, string sortColumn = null, string sortType = null, string categoryTypeFilter = null, int maximumRows = 200)
		{
			using var context = _contextFactory.CreateDbContext();
			{
				IQueryable<Category> categories = null;
				try
				{
					//categories = context.Categories.OrderBy(v => v.CategoryName);
					categories = context.Categories.Include(i => i.CustomIntelliSense).Include(i => i.Launchers).AsSingleQuery().OrderBy(v => v.CategoryName);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					return null;
				}
				if (Environment.MachineName != "J40L4V3")
				{
					categories = categories.Where(v => v.Sensitive == false);
				}
				if (searchTerm != null && searchTerm.Length > 0)
				{
					categories = categories.Where(v => v.CategoryName.Contains(searchTerm) || v.CategoryType.Contains(searchTerm));
				}
				if (sortType != null && sortColumn != null)
				{
					if (sortColumn == "CategoryName" && sortType == "Ascending")
					{
						categories = categories.OrderBy(v => v.CategoryName);
					}
					else if (sortColumn == "CategoryName" && sortType == "Descending")
					{
						categories = categories.OrderByDescending(v => v.CategoryName);
					}
				}
				if (categoryTypeFilter != null)
				{
					categories = categories.Where(v => v.CategoryType == categoryTypeFilter);
				}
				return await categories.Take(maximumRows).ToListAsync();
			}
		}


		public async Task<Category> GetCategoryAsync(int categoryId)
		{
			using var context = _contextFactory.CreateDbContext();
			Category category = await context.Categories
				.Include(i => i.Launchers)
				.Where(v => v.Id == categoryId).FirstOrDefaultAsync();
			return category;
		}
		public async Task<Category> GetCategoryAsync(int categoryId, string categoryType)
		{
			using var context = _contextFactory.CreateDbContext();
			Category category = await context.Categories.Where(v => v.Id == categoryId && v.CategoryType == categoryType).FirstOrDefaultAsync();
			return category;
		}
		public async Task<Category> GetCategoryAsync(string categoryName, string categoryType)
		{
			using var context = _contextFactory.CreateDbContext();
			Category category = await context.Categories.Where(v => v.CategoryName.ToLower() == categoryName.ToLower() && v.CategoryType == categoryType).FirstOrDefaultAsync();
			return category;
		}
		public async Task<string> SaveCategory(Category category)
		{
			using var context = _contextFactory.CreateDbContext();
			if (category.Id > 0)
			{
				context.Categories.Update(category);
			}
			else
			{
				var categories = context.Categories.Where(v => v.CategoryName.ToLower() == category.CategoryName.ToLower() && v.CategoryType.ToLower() == category.CategoryType.ToLower());
				if (categories.Count() > 0)
				{
					return $"There is already a category in the system with the same category type. This is not allowed by design. Please enter a unique category and try again.";
				}
				context.Categories.Add(category);
			}
			try
			{
				await context.SaveChangesAsync();
				return $"Category Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}

		public async Task<string> DeleteCategory(int categoryId)
		{
			using var context = _contextFactory.CreateDbContext();
			var category = await context.Categories.Where(v => v.Id == categoryId).FirstOrDefaultAsync();
			var result = $"Delete Category Failed {DateTime.UtcNow:h:mm:ss tt zz}";
			if (category != null)
			{
				context.Categories.Remove(category);
				await context.SaveChangesAsync();
				result = $"Category Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			return result;
		}
		public async Task<List<Category>> SaveAllCategories(List<Category> categories)
		{
			foreach (var category in categories)
			{
				await SaveCategory(category);
			}
			return categories;
		}
	}
}
