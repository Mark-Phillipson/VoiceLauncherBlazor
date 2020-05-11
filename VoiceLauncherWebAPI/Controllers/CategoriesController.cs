using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceLauncherWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoryService _categoryService;

        public CategoriesController(ApplicationDbContext context, CategoryService categoryService)
        {
            _categoryService = categoryService;
            _context = context;
        }
        /// <summary>
        /// Gets a list of categories :-)
        /// </summary>
        /// <returns></returns>
        // GET: api/Categories
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Category>>> GetCategoriesByTypeAsync(string categoryType)
        //{
        //    if (categoryType != null)
        //    {
        //        var categories = await _categoryService.GetCategoriesByTypeAsync(categoryType);
        //        return categories;
        //    }
        //    return NotFound();
        //}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(string searchTerm = null, string sortColumn = null, string sortType = null, string categoryTypeFilter = null, int maximumRows = 200)
        {
            var categories = await _categoryService.GetCategoriesAsync(searchTerm, sortColumn, sortType, categoryTypeFilter, maximumRows);
            return categories;
        }
        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutCategory(int id, Category category)
        //{
        //    if (id != category.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(category).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!CategoryExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Categories
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<string>> PostCategory(Category category)
        {
            var result = await _categoryService.SaveCategory(category);
            return result;
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategory(id);
            return result;
        }
        //[HttpPost]
        //public async Task<ActionResult<IEnumerable<Category>>> PostCategories(List<Category> categories)
        //{
        //    var savedCategories = await _categoryService.SaveAllCategories(categories);
        //    return categories;
        //}

    }
}
