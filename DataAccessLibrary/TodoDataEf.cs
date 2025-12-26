using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public class TodoDataEf : ITodoData
    {
        private readonly IDbContextFactory<Models.ApplicationDbContext> _dbFactory;

        public TodoDataEf(IDbContextFactory<Models.ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Todo>> GetTodos(string? searchTerm = null, string? projectFilter = null)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var query = db.Todos.AsQueryable().Where(t => !t.Archived);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // use EF.Functions.Like to simulate SQL LIKE "%term%"
                query = query.Where(t => EF.Functions.Like(t.Title, $"%{searchTerm}%") || EF.Functions.Like(t.Description, $"%{searchTerm}%"));
            }
            if (!string.IsNullOrEmpty(projectFilter))
            {
                query = query.Where(t => t.Project == projectFilter);
            }
            var results = await query
                .OrderBy(t => t.Completed)
                .ThenByDescending(t => t.SortPriority)
                .ThenByDescending(t => t.Created)
                .ToListAsync();

            return results;
        }

        public async Task<List<string>> GetProjects()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var projects = await db.Todos
                .Where(t => !t.Archived && !t.Completed && !string.IsNullOrEmpty(t.Project))
                .Select(t => t.Project!)
                .Distinct()
                .ToListAsync();
            return projects;
        }

        public async Task<Todo> GetTodo(int Id)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == Id);
            if (todo == null)
            {
                throw new System.ArgumentNullException("data", "data is null");
            }
            return todo;
        }

        public async Task InsertToDo(Todo todo)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            todo.Created = todo.Created == default ? System.DateTime.Now : todo.Created;
            db.Todos.Add(todo);
            await db.SaveChangesAsync();
        }

        public async Task UpdateToDo(Todo todo)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var existing = await db.Todos.FirstOrDefaultAsync(t => t.Id == todo.Id);
            if (existing == null)
            {
                throw new System.ArgumentNullException("data", "data is null");
            }
            existing.Title = todo.Title;
            existing.Description = todo.Description;
            existing.Completed = todo.Completed;
            existing.Project = todo.Project;
            existing.Archived = todo.Archived;
            existing.SortPriority = todo.SortPriority;
            await db.SaveChangesAsync();
        }

        public async Task DeleteToDo(Todo todo)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var existing = await db.Todos.FirstOrDefaultAsync(t => t.Id == todo.Id);
            if (existing == null)
            {
                return;
            }
            db.Todos.Remove(existing);
            await db.SaveChangesAsync();
        }
    }
}
