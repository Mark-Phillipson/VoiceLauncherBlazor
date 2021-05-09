using Blazored.Toast.Services;
using DataAccessLibrary.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Pages
{
	public partial class TodosPage
	{
		[Parameter] public string Project { get; set; }
		private List<DataAccessLibrary.Models.Todo> todos;
		[Inject] IToastService ToastService { get; set; }
		private Todo todo = null;
		public bool ShowDialog { get; set; }
		public bool ShowDialogArchive { get; set; }
		public int todoIdDelete { get; set; }
		public int todoIdArchive { get; set; }
		public string SearchTerm { get; set; } = null;
		public bool _loadFailed { get; set; }
		public List<string> projects { get; set; } = new List<string>();
		public string ProjectFilter { get; set; }
		public string StatusMessage { get; set; }
		private int percentDone
		{
			get
			{
				return (todos.Count(x => x.Completed) * 100) / todos.Count;
			}
		}
		protected override async Task OnInitializedAsync()
		{
			todo = new Todo { Project = Project };
			await LoadData();
		}
		private void CancelEdit()
		{
			todo = new Todo { Project = Project };
			JSRuntime.InvokeVoidAsync("setFocus", $"SearchInput");
		}
		private void EditTodo(Todo todoEdit)
		{
			todo.Id = todoEdit.Id;
			todo.Title = todoEdit.Title;
			todo.Description = todoEdit.Description;
			todo.Completed = todoEdit.Completed;
			todo.Project = todoEdit.Project;
			todo.Archived = todoEdit.Archived;
			JSRuntime.InvokeVoidAsync("setFocus", $"{todo.Id.ToString()}Title");
		}
		void ConfirmArchive(int todoId)
		{
			ShowDialogArchive = true;
			todoIdArchive = todoId;
		}
		void ConfirmDelete(int todoId)
		{
			ShowDialog = true;
			todoIdDelete = todoId;
		}
		void CancelDialog()
		{
			ShowDialog = false;
			ShowDialogArchive = false;
		}

		private async Task DeleteTodo(int todoId)
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			Todo todo = await TodoData.GetTodo(todoId);
			await TodoData.DeleteToDo(todo);
			await LoadData();
			ShowDialog = false;
			StatusMessage = $"Deleted Successfully {DateTime.UtcNow:h:mm:ss tt zz}";
		}
		private async Task ArchiveTodo(int todoId)
		{
			Todo todo = await TodoData.GetTodo(todoId);
			todo.Archived = true;
			await TodoData.UpdateToDo(todo);
			await LoadData();
			ShowDialogArchive = false;
			StatusMessage = $"Archived Successfully {DateTime.UtcNow:h:mm:ss tt zz}";
		}
		async Task ApplyFilter()
		{
			await LoadData();
		}
		async Task LoadData()
		{
			if (ProjectFilter == "")
			{
				ProjectFilter = null;
			}
			try
			{
				if (Project != null)
				{
					ProjectFilter = Project;
				}
				todos = await TodoData.GetTodos(SearchTerm, ProjectFilter);
				projects = await TodoData.GetProjects();
				StatusMessage = $"Data Loaded Successfully {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		private async Task InsertTodo()
		{
			await CallChangeAsync(todo.Id.ToString() + "Project");
			if (todo.Id > 0)
			{
				await SaveTodo();
			}
			else
			{
				await TodoData.InsertToDo(todo);
				await LoadData();
				StatusMessage = $"Created Successfully {DateTime.UtcNow:h:mm:ss tt zz}";
				todo = new Todo { Project = Project };
			}
		}
		private async Task ChangeCompleted(int todoId, bool completed)
		{
			todo = todos.Where(v => v.Id == todoId).FirstOrDefault();
			todo.Completed = completed;
			await TodoData.UpdateToDo(todo);
			await LoadData();
		}
		private async Task SaveTodo()
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			await TodoData.UpdateToDo(todo);
			await LoadData();
			StatusMessage = $"Updated Successfully {DateTime.UtcNow:h:mm:ss tt zz}";
			todo = new Todo { Project = Project };
		}

		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
			await ApplyFilter();
		}
	}
}