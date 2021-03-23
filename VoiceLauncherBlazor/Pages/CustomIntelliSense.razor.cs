using Blazored.Toast.Services;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceLauncherBlazor.helpers;

namespace VoiceLauncherBlazor.Pages
{
	public partial class CustomIntelliSense
	{
		[Parameter] public int customIntellisenseId { get; set; } = 0;
		[Inject] NavigationManager NavigationManager { get; set; }
		[Inject] IToastService ToastService { get; set; }
#pragma warning disable 414
		private bool _loadFailed = false;
#pragma warning restore 414
		public DataAccessLibrary.Models.CustomIntelliSense intellisense { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup> generalLookups { get; set; }
		public List<DataAccessLibrary.Models.Language> languages { get; set; }
		public List<DataAccessLibrary.Models.Category> categories { get; set; }
		private List<string> customValidationErrors = new List<string>();
		public string Message { get; set; }
		[Parameter]
		public int? LanguageIdDefault { get; set; }
		[Parameter]
		public int? CategoryIdDefault { get; set; }
		protected override async Task OnInitializedAsync()
		{
			if (customIntellisenseId > 0)
			{
				try
				{
					intellisense = await CustomIntellisenseService.GetCustomIntelliSenseAsync(customIntellisenseId);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
			else
			{
				intellisense = new DataAccessLibrary.Models.CustomIntelliSense
				{
					DeliveryType = "Send Keys"
				};
				var query = new Uri(NavigationManager.Uri).Query;
				if (QueryHelpers.ParseQuery(query).TryGetValue("languageId", out var languageId))
				{
					LanguageIdDefault = Int32.Parse(languageId);
				}
				if (QueryHelpers.ParseQuery(query).TryGetValue("categoryId", out var categoryId))
				{
					CategoryIdDefault = Int32.Parse(categoryId);
				}
				if (LanguageIdDefault!=  null )
				{
					intellisense.LanguageId = (int)LanguageIdDefault;
				}
				if (CategoryIdDefault!= null )
				{
					intellisense.CategoryId = (int)CategoryIdDefault;
				}
			}
			generalLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Delivery Type");
			languages = (await LanguageService.GetLanguagesAsync(activeFilter:true));
			categories = await CategoryService.GetCategoriesAsync();
		}

		protected override async Task OnParametersSetAsync()
		{
			if (intellisense.Id > 0)
			{
				intellisense = await CustomIntellisenseService.GetCustomIntelliSenseAsync(intellisense.Id);
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await JSRuntime.InvokeVoidAsync("setFocus", "LanguageSelect");
			}
		}
		private async Task HandleValidSubmit()
		{
			intellisense.SendKeysValue = CarriageReturn.ReplaceForCarriageReturnChar(intellisense.SendKeysValue);
			customValidationErrors.Clear();
			if (intellisense.LanguageId == 0)
			{
				customValidationErrors.Add("Language is required");
			}
			if (intellisense.CategoryId == 0)
			{
				customValidationErrors.Add("Category is required");
			}
			if (customValidationErrors.Count == 0)
			{
				var result = await CustomIntellisenseService.SaveCustomIntelliSense(intellisense);
				if (result.Contains("Successfully"))
				{
					ToastService.ShowSuccess(result, "Success");
					return;
				}
				ToastService.ShowError(result, "Failure");
			}
		}
		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
		}
		private void GoBack()
		{
			NavigationManager.NavigateTo("/intellisenses");
		}
		//public async Task<IEnumerable<DataAccessLibrary.Models.Language>> FilterLanguages(string searchText)
		//{
		//	var languages = await LanguageService.GetLanguagesAsync(searchText);
		//	return languages;
		//}
		//public int? GetLanguageId(DataAccessLibrary.Models.Language language)
		//{
		//	return language?.Id;
		//}
		//public DataAccessLibrary.Models.Language LoadSelectedLanguage(int? languageId)
		//{
		//	if (languageId != null)
		//	{
		//		var language = languages.FirstOrDefault(l => l.Id == languageId);
		//		return language;
		//	}
		//	return null;
		//}

	}
}
