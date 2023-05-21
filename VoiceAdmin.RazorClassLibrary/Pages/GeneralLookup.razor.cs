using Blazored.Toast.Services;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace RazorClassLibrary.Pages
{
    public partial class GeneralLookup : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [Parameter] public int? GeneralLookupId { get; set; }
        private EditContext? _editContext;
        public DataAccessLibrary.Models.GeneralLookup? GeneralLookupModel { get; set; }
        public List<DataAccessLibrary.Models.GeneralLookup>? GeneralLookups { get; set; }
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414

        protected override async Task OnInitializedAsync()
        {
            if (GeneralLookupId > 0)
            {
                try
                {
                    if (GeneralLookupId != null)
                    {
                        GeneralLookupModel = await GeneralLookupService.GetGeneralLookupAsync((int)GeneralLookupId);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
            else
            {
                GeneralLookupModel = new DataAccessLibrary.Models.GeneralLookup
                {
                    Category = "Default?",
                    SortOrder = 1

                };
            }
            if (GeneralLookupModel != null)
            {
                _editContext = new EditContext(GeneralLookupModel);
            }
            try
            {
                GeneralLookups = await GeneralLookupService.GetGeneralLookUpsAsync(GeneralLookupModel!.Category);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (GeneralLookupId > 0)
            {
                try
                {
                    GeneralLookupModel = await GeneralLookupService.GetGeneralLookupAsync((int)GeneralLookupId);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
        }

        private async Task HandleValidSubmit()
        {
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
                return;
            }
            try
            {
                var result = await GeneralLookupService.SaveGeneralLookup(GeneralLookupModel);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
            NavigationManager.NavigateTo("generallookups");
        }
    }
}
