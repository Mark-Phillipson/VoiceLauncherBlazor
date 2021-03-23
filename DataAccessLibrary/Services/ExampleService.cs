using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Services
{
    public class ExampleService
    {
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public ExampleService(IDbContextFactory<ApplicationDbContext> context)
        {
            this._contextFactory = context;
        }
        public IQueryable<Example> GetExamples(string searchTerm = null, string sortColumn = null, string sortType = null, string categoryTypeFilter = null, int maximumRows = 200)
        {
			using var context = _contextFactory.CreateDbContext();
			IQueryable<Example> examples = null;
            try
            {
                examples = context.Examples.OrderBy(v => v.Text);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            if (searchTerm != null && searchTerm.Length > 0)
            {
                examples = examples.Where(v => v.Text.Contains(searchTerm) || v.LargeText.Contains(searchTerm));
            }
            if (sortType != null && sortColumn != null)
            {
                if (sortColumn == "Text" && sortType == "Ascending")
                {
                    examples = examples.OrderBy(v => v.Text);
                }
                else if (sortColumn == "Text" && sortType == "Descending")
                {
                    examples = examples.OrderByDescending(v => v.Text);
                }
            }
            //if (categoryTypeFilter != null)
            //{
            //    examples = examples.Where(v => v.Number == categoryTypeFilter);
            //}
            return examples.Take(maximumRows);
        }


        public async Task<Example> GetExampleAsync(int exampleId)
        {
			using var context = _contextFactory.CreateDbContext();
			Example example = await context.Examples.Where(v => v.Id == exampleId).FirstOrDefaultAsync();
            return example;
        }
        public async Task<string> SaveExample(Example example)
        {
			using var context = _contextFactory.CreateDbContext();
			if (example.Id > 0)
            {
                context.Examples.Update(example);
            }
            else
            {
                context.Examples.Add(example);
            }
            try
            {
                await context.SaveChangesAsync();
                return $"Example Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public async Task<string> DeleteExample(int exampleId)
        {
			using var context = _contextFactory.CreateDbContext();
			var example = await context.Examples.Where(v => v.Id == exampleId).FirstOrDefaultAsync();
            var result = $"Delete Example Failed {DateTime.UtcNow:h:mm:ss tt zz}";
            if (example != null)
            {
                context.Examples.Remove(example);
                await context.SaveChangesAsync();
                result = $"Example Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            return result;
        }
        public async Task<List<Example>> SaveAllExamples(List<Example> examples)
        {
            foreach (var example in examples)
            {
                await SaveExample(example);
            }
            return examples;
        }
    }
}
