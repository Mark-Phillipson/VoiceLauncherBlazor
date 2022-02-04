using DataAccessLibrary;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncher.Pages
{
	public partial class TasksBreakdown
	{
		[Inject] ITodoData TodoData { get; set; }

		public List<string> projects { get; set; } = new List<string>();
		protected override async Task OnInitializedAsync()
		{
			projects = await TodoData.GetProjects();
		}

	}
}