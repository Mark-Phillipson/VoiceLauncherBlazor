using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
	public class TodoData : ITodoData
	{
		private string table = "dbo.Todos";
		private string[] columns = new string[] { "Title", "Description", "Completed", "Project", "Archived","SortPriority" };
		private readonly ISqlDataAccess _db;
		public TodoData(ISqlDataAccess db)
		{
			_db = db;
		}
		public Task<List<Todo>> GetTodos(string searchTerm = null, string projectFilter = null)
		{
			string sql = $"SELECT * FROM {table} WHERE Archived=0";
			if (searchTerm != null)
			{
				sql = sql + $" And (Title Like '%{searchTerm}%' Or Description Like '%{searchTerm}%')";
			}
			if (projectFilter != null)
			{
				sql = sql + $" And Project='{projectFilter}'";
			}
			sql = sql + $" ORDER BY Completed, SortPriority DESC, Created DESC;";
			return _db.LoadData<Todo, dynamic>(sql, new { });
		}
		public Task<List<string>> GetProjects()
		{
			string sql = $"SELECT Project FROM {table} WHERE Archived=0 And Project IS NOT NULL AND Completed=0 GROUP BY Project";
			return _db.LoadData<string, dynamic>(sql, new { });
		}
		public Task<Todo> GetTodo(int Id)
		{
			string sql = $"SELECT * FROM {table} WHERE Id={Id}";
			return _db.LoadSingleData<Todo, dynamic>(sql, new { });
		}
		public Task InsertToDo(Todo todo)
		{
			string columns1 = "";
			string columns2 = "";
			int counter = 0;
			foreach (var column in columns)
			{
				counter++;
				columns1 = columns1 + column + (counter < columns.Count() ? ", " : "");
				columns2 = columns2 + "@" + column + (counter < columns.Count() ? ", " : "");
			}
			string sql = $"insert into {table} ({columns1}) " +
				$"values ({columns2})";
			return _db.SaveData<Todo>(sql, todo);
		}
		public Task UpdateToDo(Todo todo)
		{
			string columns1 = "";
			int counter = 0;
			foreach (var column in columns)
			{
				counter++;
				columns1 = $"{columns1} [{column}]=@{column}" + (counter < columns.Count() ? ", " : "");
			}
			string sql = $"UPDATE {table} SET {columns1} " +
				$"WHERE Id=@Id";
			return _db.SaveData<Todo>(sql, todo);
		}
		public Task DeleteToDo(Todo todo)
		{
			string sql = $"DELETE FROM {table} WHERE Id=@Id";
			return _db.SaveData<Todo>(sql, todo);
		}
	}
}
