using Blazored.Toast.Services;

using DataAccessLibrary.Models;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

using RazorClassLibrary.helpers;

namespace RazorClassLibrary.Pages
{
  public partial class CustomIntelliSenseEdit : ComponentBase
  {
    [Parameter] public int CustomIntellisenseId { get; set; } = 0;
    [Inject] NavigationManager? NavigationManager { get; set; }
    [Inject] IToastService? ToastService { get; set; }
    [Inject] public required CustomIntellisenseService CustomIntellisenseService { get; set; }
    [Inject] public required CategoryService CategoryService { get; set; }
    [Inject] public required LanguageService LanguageService { get; set; }
    [Inject] public required GeneralLookupService GeneralLookupService { get; set; }

    [Inject] public required IJSRuntime JSRuntime { get; set; }

#pragma warning disable 414
    private bool _loadFailed = false;
#pragma warning restore 414
    public CustomIntelliSense? Intellisense { get; set; }


    public List<DataAccessLibrary.Models.GeneralLookup>? GeneralLookups { get; set; }
    public List<Language>? Languages { get; set; }
    public List<Category>? Categories { get; set; }

    private static readonly List<string> list = new();
    readonly List<string> customValidationErrors = list;
    public string? Message { get; set; }
    [Parameter]
    public int? LanguageIdDefault { get; set; }
    [Parameter]
    public int? CategoryIdDefault { get; set; }
    public int? SelectedLanguageId { get; set; } = 0;
    public int? SelectedCategoryId { get; set; } = 0;
    protected override async Task OnInitializedAsync()
    {
      if (CustomIntellisenseId > 0)
      {
        try
        {
          Intellisense = await CustomIntellisenseService.GetCustomIntelliSenseAsync(CustomIntellisenseId);
        }
        catch (Exception exception)
        {
          Console.WriteLine(exception.Message);
          _loadFailed = true;
        }
        SelectedCategoryId = Intellisense!.CategoryId;
        SelectedLanguageId = Intellisense.LanguageId;
      }
      else
      {
        Intellisense = new CustomIntelliSense
        {
          DeliveryType = "Send Keys"
        };
        var query = new Uri(NavigationManager!.Uri).Query;
        if (QueryHelpers.ParseQuery(query).TryGetValue("languageId", out var languageId))
        {
          LanguageIdDefault = int.Parse($"{languageId}");
        }
        if (QueryHelpers.ParseQuery(query).TryGetValue("categoryId", out var categoryId))
        {
          if (categoryId.ToString() != null)
          {
            CategoryIdDefault = int.Parse($"{categoryId}");
          }
        }
        if (LanguageIdDefault != null)
        {
          Intellisense.LanguageId = (int)LanguageIdDefault;
          SelectedLanguageId = LanguageIdDefault;
        }
        if (CategoryIdDefault != null)
        {
          Intellisense.CategoryId = (int)CategoryIdDefault;
          SelectedCategoryId = CategoryIdDefault;
        }
      }
      GeneralLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Delivery Type");
      Languages = await LanguageService.GetLanguagesAsync(activeFilter: true);
      Categories = await CategoryService.GetCategoriesAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
      if (Intellisense!.Id > 0)
      {
        Intellisense = await CustomIntellisenseService.GetCustomIntelliSenseAsync(Intellisense.Id);
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
      Intellisense!.SendKeysValue = CarriageReturn.ReplaceForCarriageReturnChar(Intellisense.SendKeysValue);
      customValidationErrors.Clear();
      if (Intellisense.LanguageId == 0)
      {
        customValidationErrors.Add("Language is required");
      }
      if (Intellisense.CategoryId == 0)
      {
        customValidationErrors.Add("Category is required");
      }
      if (customValidationErrors.Count == 0)
      {

        var result = await CustomIntellisenseService.SaveCustomIntelliSense(Intellisense);
        if (result.Contains("Successfully"))
        {
          ToastService!.ShowSuccess(result, "Success");
          return;
        }
        ToastService!.ShowError(result, "Failure");
      }
    }
    private async Task CallChangeAsync(string elementId)
    {
      await JSRuntime.InvokeVoidAsync("CallChange", elementId);
    }
    private async Task GoBackAsync()
    {
      if (Intellisense!.Language == null && Intellisense.LanguageId > 0)
      {
        Intellisense.Language = await LanguageService.GetLanguageAsync(Intellisense.LanguageId);
      }
      if (Intellisense.Category == null && Intellisense.CategoryId > 0)
      {
        Intellisense.Category = await CategoryService.GetCategoryAsync(Intellisense.CategoryId);
      }

      if (Intellisense.Language != null && Intellisense.Category != null)
      {
        NavigationManager!.NavigateTo($"/intellisenses?language={Intellisense.Language.LanguageName}&category={Intellisense.Category.CategoryName}");
      }
    }
    public async Task<IEnumerable<Language>> FilterLanguages(string searchText)
    {
      return await LanguageService.GetLanguagesAsync(searchText);
    }
    public async Task<IEnumerable<Category>> FilterCategories(string searchText)
    {
      var result = await CategoryService.GetCategoriesAsync(searchText);
      return result;
    }
    //public int? GetLanguageId(Language language)
    //{
    //	return language?.Id;
    //}
    //public int? GetCategoryId(Category category)
    //{
    //	return category?.Id;
    //}
    public Language? LoadSelectedLanguage(int? languageId)
    {
      if (languageId != null && Languages != null)
      {
        var language = Languages.FirstOrDefault(l => l.Id == languageId);
        return language;
      }
      return null;
    }
    void SetLanguage(FocusEventArgs args)
    {
      if (SelectedLanguageId != null)
      {
        Intellisense!.LanguageId = (int)SelectedLanguageId;
      }
    }
    void SetCategory(FocusEventArgs args)
    {
      if (SelectedCategoryId != null)
      {
        Intellisense!.CategoryId = (int)SelectedCategoryId;
      }
    }
    public Category? LoadSelectedCategory(int? categoryId)
    {
      if (categoryId != null && Categories != null)
      {
        var category = Categories.FirstOrDefault(c => c.Id == categoryId);
        return category;
      }
      return null;
    }

  }
}
