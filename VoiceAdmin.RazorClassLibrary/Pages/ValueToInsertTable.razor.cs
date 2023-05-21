using Ardalis.GuardClauses;

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Shared;
using System.Security.Claims;

namespace RazorClassLibrary.Pages
{
    public partial class ValueToInsertTable : ComponentBase
    {
        [Inject] public IValueToInsertDataService? ValueToInsertDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<ValueToInsertTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        //[Inject] public ApplicationState? ApplicationState { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "ValueToInsert Items (ValuesToInsert)";
        public string EditTitle { get; set; } = "Edit ValueToInsert Item (ValuesToInsert)";
        [Parameter] public int ParentId { get; set; }
        public List<ValueToInsertDTO>? ValueToInsertDTO { get; set; }
        public List<ValueToInsertDTO>? FilteredValueToInsertDTO { get; set; }
        protected ValueToInsertAddEdit? ValueToInsertAddEdit { get; set; }
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
        [Parameter] public string? ServerSearchTerm { get; set; }
        public string ExceptionMessage { get; set; } = string.Empty;
        public List<string>? PropertyInfo { get; set; }
        [CascadingParameter] public ClaimsPrincipal? User { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        public bool ShowEdit { get; set; } = false;
        private bool ShowDeleteConfirm { get; set; }
        private int ValueToInsertId { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (ValueToInsertDataService != null)
                {
                    var result = await ValueToInsertDataService!.GetAllValuesToInsertAsync();
                    //var result = await ValueToInsertDataService.SearchValuesToInsertAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        ValueToInsertDTO = result.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredValueToInsertDTO = ValueToInsertDTO;
            Title = $"Value To Insert ({FilteredValueToInsertDTO?.Count})";

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    if (JSRuntime != null)
                    {
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }
        private void AddNewValueToInsert()
        {
            //            var parameters = new ModalParameters();
            //            var formModal = Modal?.Show<ValueToInsertAddEdit>("Add Value To Insert", parameters);
            //            if (formModal != null)
            //            {
            //                var result = await formModal.Result;
            //                if (!result.Cancelled)
            //                {
            //                    await LoadData();
            //                }
            //            }
            ValueToInsertId = 0;
            EditTitle = "Add Value To Insert";
            ShowEdit = true;
        }

        private void ApplyFilter()
        {
            if (FilteredValueToInsertDTO == null || ValueToInsertDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredValueToInsertDTO = ValueToInsertDTO.OrderBy(v => v.ValueToInsertValue).ToList();
                Title = $"All Value To Insert ({FilteredValueToInsertDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredValueToInsertDTO = ValueToInsertDTO
                    .Where(v =>
                    v.ValueToInsertValue != null && v.ValueToInsertValue.ToLower().Contains(temporary)
                     || v.Lookup != null && v.Lookup.ToLower().Contains(temporary)
                     || v.Description != null && v.Description.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Value To Inserts ({FilteredValueToInsertDTO.Count})";
            }
        }
        protected void SortValueToInsert(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredValueToInsertDTO == null)
            {
                return;
            }
            if (sortColumn == "ValueToInsert")
            {
                FilteredValueToInsertDTO = FilteredValueToInsertDTO.OrderBy(v => v.ValueToInsertValue).ToList();
            }
            else if (sortColumn == "ValueToInsert Desc")
            {
                FilteredValueToInsertDTO = FilteredValueToInsertDTO.OrderByDescending(v => v.ValueToInsertValue).ToList();
            }
            if (sortColumn == "Lookup")
            {
                FilteredValueToInsertDTO = FilteredValueToInsertDTO.OrderBy(v => v.Lookup).ToList();
            }
            else if (sortColumn == "Lookup Desc")
            {
                FilteredValueToInsertDTO = FilteredValueToInsertDTO.OrderByDescending(v => v.Lookup).ToList();
            }
        }
        private async Task DeleteValueToInsertAsync(int Id)
        {
            var parameters = new ModalParameters();
            if (ValueToInsertDataService != null)
            {
                var valueToInsert = await ValueToInsertDataService.GetValueToInsertById(Id);
                parameters.Add("Title", "Please Confirm, Delete");
                parameters.Add("Message", $": {valueToInsert?.ValueToInsertValue}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Value to Insert ({valueToInsert?.ValueToInsertValue})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await ValueToInsertDataService.DeleteValueToInsert(Id);
                        ToastService?.ShowSuccess(" Value to insert deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
        }
        private void EditValueToInsert(int Id)
        {
            //            var parameters = new ModalParameters();
            //parameters.Add("Id", Id);
            //var formModal = Modal?.Show<ValueToInsertAddEdit>("Edit Value To Insert", parameters);
            //if (formModal != null)
            //{
            //var result = await formModal.Result;
            //if (!result.Cancelled)
            //{
            //    await LoadData();
            //}
            //}
            ValueToInsertId = Id;
            EditTitle = "Edit Value To Insert";
            ShowEdit = true;
        }
        private void ToggleModal()
        {
            ShowEdit = !ShowEdit;
        }
        private void ToggleShowDeleteConfirm()
        {
            ShowDeleteConfirm = !ShowDeleteConfirm;
        }
        public async Task CloseModalAsync(bool close)
        {
            if (close)
            {
                ShowEdit = false;
                await LoadData();
            }
        }
        private async void CloseConfirmDeletion(bool confirmation)
        {
            ShowDeleteConfirm = false;
            if (ValueToInsertDataService == null) return;
            if (confirmation)
            {
                await ValueToInsertDataService.DeleteValueToInsert(ValueToInsertId);
                if (ToastService != null)
                {
                    ToastService.ShowSuccess($"{ValueToInsertId} Value To Insert item has been deleted successfully");
                }

            }
            await LoadData();
            StateHasChanged();
        }
    }
}