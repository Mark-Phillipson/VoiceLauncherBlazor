using DataAccessLibrary;
using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Pages
{
    public partial class TasksBreakdown : ComponentBase
    {
        [Inject] ITodoData? TodoData { get; set; }

        public List<string> Projects { get; set; } = new List<string>();
        protected override async Task OnInitializedAsync()
        {
            Projects = await TodoData!.GetProjects();
        }

    }
}