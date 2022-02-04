using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace VoiceLauncherBlazor.Data
{
    public class ExampleService
    {
        readonly ApplicationDbContext _context;
        public ExampleService(ApplicationDbContext context)
        {
            this._context = context;
        }
        public IQueryable<Example> GetExamples(string searchTerm = null, string sortColumn = null, string sortType = null, string categoryTypeFilter = null, int maximumRows = 200)
        {
            IQueryable<Example> examples = null;
            try
            {
                examples = _context.Examples.OrderBy(v => v.Text);
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
            Example example = await _context.Examples.Where(v => v.Id == exampleId).FirstOrDefaultAsync();
            return example;
        }
        public async Task<string> SaveExample(Example example)
        {
            if (example.Id > 0)
            {
                _context.Examples.Update(example);
            }
            else
            {
                _context.Examples.Add(example);
            }
            try
            {
                await _context.SaveChangesAsync();
                return $"Example Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public async Task<string> DeleteExample(int exampleId)
        {
            var example = await _context.Examples.Where(v => v.Id == exampleId).FirstOrDefaultAsync();
            var result = $"Delete Example Failed {DateTime.UtcNow:h:mm:ss tt zz}";
            if (example != null)
            {
                _context.Examples.Remove(example);
                await _context.SaveChangesAsync();
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
