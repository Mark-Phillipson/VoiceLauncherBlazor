using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface ITodoData
    {
        Task<List<Todo>> GetTodos(string searchTerm = null, string projectFilter = null);
        Task<List<string>> GetProjects();
        Task<Todo> GetTodo(int Id);
        Task InsertToDo(Todo todo);
        Task UpdateToDo(Todo todo);
        Task DeleteToDo(Todo todo);
    }
}
