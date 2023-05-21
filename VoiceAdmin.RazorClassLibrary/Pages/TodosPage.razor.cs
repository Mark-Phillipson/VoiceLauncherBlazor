using Blazored.Toast.Services;
using DataAccessLibrary.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Pages
{
    public partial class TodosPage : ComponentBase
    {
        [Parameter] public string? Project { get; set; }
        private List<Todo>? todos;
        [Inject] IToastService? ToastService { get; set; }
        private Todo? todo = null;
        public bool ShowDialog { get; set; }
        public bool ShowDialogArchive { get; set; }
        public int TodoIdDelete { get; set; }
        public int TodoIdArchive { get; set; }
        public string SearchTerm { get; set; } = "";
        private bool LoadFailed { get; set; }
        public List<string> Projects { get; set; } = new List<string>();
        public string? ProjectFilter { get; set; }
        public string? StatusMessage { get; set; }
        public bool ShowProjects { get; set; } = false;
        private int PercentDone
        {
            get
            {
                return todos!.Count(x => x.Completed) * 100 / todos!.Count;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            todo = new Todo { Project = Project };
            await LoadData();
        }
        private async void CancelEdit()
        {
            todo = new Todo { Project = Project };
            await JSRuntime!.InvokeVoidAsync("setFocus", $"SearchInput");
        }
        private async void EditTodo(Todo todoEdit)
        {
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
                return;
            }
            todo!.Id = todoEdit.Id;
            todo.Title = todoEdit.Title;
            todo.Description = todoEdit.Description;
            todo.Completed = todoEdit.Completed;
            todo.Project = todoEdit.Project;
            todo.Archived = todoEdit.Archived;
            await JSRuntime!.InvokeVoidAsync("setFocus", $"{todo!.Id}Title");
        }
        void ConfirmArchive(int todoId)
        {
            ShowDialogArchive = true;
            TodoIdArchive = todoId;
        }
        void ConfirmDelete(int todoId)
        {
            ShowDialog = true;
            TodoIdDelete = todoId;
        }
        void CancelDialog()
        {
            ShowDialog = false;
            ShowDialogArchive = false;
        }

        private async Task DeleteTodo(int todoId)
        {
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
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
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
                return;
            }
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
                Projects = await TodoData.GetProjects();
                StatusMessage = $"Data Loaded Successfully {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                LoadFailed = true;
            }
        }
        private async Task InsertTodo()
        {
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
                return;
            }
            await CallChangeAsync(todo!.Id.ToString() + "Project");
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
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
                return;
            }
            todo = todos!.Where(v => v.Id == todoId).FirstOrDefault();
            todo!.Completed = completed;
            await TodoData.UpdateToDo(todo);
            await LoadData();
        }
        private async Task SaveTodo()
        {
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
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
        async Task SetPriorityAsync(int todoId, bool increase)
        {
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
                return;
            }
            todo = await TodoData.GetTodo(todoId);
            if (increase)
            {
                todo.SortPriority++;
            }
            else
            {
                todo.SortPriority--;
            }
            await TodoData.UpdateToDo(todo);
            await LoadData();
            StatusMessage = $"Priority Updated Successfully {DateTime.UtcNow:h:mm:ss tt zz}";
            todo = new Todo { Project = Project };
        }
        void ToggleShowProjects()
        {
            ShowProjects = !ShowProjects;
        }
        async Task FilterByProjectAsync(string project)
        {
            ProjectFilter = project;
            await ApplyFilter();
        }
    }
}