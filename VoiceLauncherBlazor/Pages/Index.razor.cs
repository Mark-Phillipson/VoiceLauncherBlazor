using Blazored.Toast.Services;
using DataAccessLibrary;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Pages
{
	public partial class Index
	{
		[Inject] ITodoData TodoData { get; set; }
		[Inject] IToastService ToastService { get; set; }
		public List<string> projects { get; set; } = new List<string>();
		protected override async Task OnInitializedAsync()
		{
			projects = await TodoData.GetProjects();
			ToastService.ShowInfo("To Blazor Server demonstration application", "Hello and Welcome");
		}
	}
}