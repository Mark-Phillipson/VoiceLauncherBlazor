
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Services;
using System.Security.Claims;
using Ardalis.GuardClauses;
using RazorClassLibrary.Shared;
using DataAccessLibrary.Services;
using DataAccessLibrary.DTOs;
using Microsoft.Extensions.Logging;
using SmartComponents.LocalEmbeddings;

namespace RazorClassLibrary.Pages
{
    public partial class CssPropertyTable : ComponentBase
    {
        [Inject] public required ICssPropertyDataService CssPropertyDataService { get; set; }
        [Inject] public required NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<CssPropertyTable>? Logger { get; set; }

        [Inject] public required IToastService ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "CssProperty Items (CssProperties)";
        public string EditTitle { get; set; } = "Edit CssProperty Item (CssProperties)";
        [Parameter] public int ParentId { get; set; }
        public List<CssPropertyDTO>? CssPropertyDTO { get; set; }
        public List<CssPropertyDTO>? FilteredCssPropertyDTO { get; set; }
        protected CssPropertyAddEdit? CssPropertyAddEdit { get; set; }
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; } }
        private string? clientSearchedTerm { get; set; }
        public string? ClientSearchTerm { get => clientSearchedTerm; set { clientSearchedTerm = value; ApplyLocalFilter(); } }
        private bool _serverPaging = false;
        private void ApplyLocalFilter()
        {
            if (FilteredCssPropertyDTO == null || CssPropertyDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(ClientSearchTerm))
            {
                FilteredCssPropertyDTO = CssPropertyDTO;
            }
            else
            {
                if (useSemanticMatching)
                {
                    var propertyNameAndDescription = CssPropertyDTO.Where(f => f.PropertyName != null && f.Description != null).Select(v => $"{v.PropertyName} | {v.Description}").ToList();
                    using var localCommandEmbedder = new LocalEmbedder();
                    IList<(string Item, EmbeddingF32 Embedding)> matchedResults;
                    matchedResults = localCommandEmbedder.EmbedRange(
                        propertyNameAndDescription.ToList());
                    SimilarityScore<string>[] results = LocalEmbedder.FindClosestWithScore(localCommandEmbedder.Embed(ClientSearchTerm), matchedResults, maxResults: maxResults);
                    if (results.Length > 0)
                    {
                        var resultItems = results.Select(r => r.Item.Split('|')[0].Trim()).ToList();
                        var similarityScores = results.ToDictionary(r => r.Item.Split('|')[0].Trim(), r => r.Similarity);

                        FilteredCssPropertyDTO = CssPropertyDTO
                            .Where(v => resultItems.Contains(v.PropertyName)
                             || v.Description != null && resultItems.Contains(v.Description)
                            )
                            .OrderByDescending(v => similarityScores[v.PropertyName])
                            .ToList();
                    }
                }
                else
                {
                    FilteredCssPropertyDTO = CssPropertyDTO.Where(v => v.PropertyName?.Contains(ClientSearchTerm, StringComparison.OrdinalIgnoreCase) == true
                    || v.Description?.Contains(ClientSearchTerm, StringComparison.OrdinalIgnoreCase) == true
                    ).ToList();
                }
            }
            Title = $"Filtered CSS Properties ({FilteredCssPropertyDTO.Count})";
        }

        private string? lastSearchTerm { get; set; }

        [Parameter] public string? ServerSearchTerm { get; set; }
        public string ExceptionMessage { get; set; } = String.Empty;
        public List<string>? PropertyInfo { get; set; }
        [CascadingParameter] public ClaimsPrincipal? User { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        public bool ShowEdit { get; set; } = false;
        private bool ShowDeleteConfirm { get; set; }
        private int pageNumber = 1;
        private int pageSize = 1000;
        private int totalRows = 0;
        private bool useSemanticMatching = true;
        private int maxResults = 20;

        private int CssPropertyId { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (CssPropertyDataService != null)
                {
                    totalRows = await CssPropertyDataService.GetTotalCount();
                    var result = await CssPropertyDataService!.GetAllCssPropertiesAsync

                    (pageNumber, pageSize, ServerSearchTerm ?? "");
                    if (result != null)
                    {
                        CssPropertyDTO = result.ToList();
                        FilteredCssPropertyDTO = result.ToList();
                        StateHasChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.LogError("e, Exception occurred in LoadData Method, Getting Records from the Service");
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredCssPropertyDTO = CssPropertyDTO;
            Title = $"CSS Properties ({FilteredCssPropertyDTO?.Count})";

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await Task.Delay(100);
                    await SearchInput.FocusAsync();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }
        private async Task AddNewCssProperty()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<CssPropertyAddEdit>("Add CSS Property", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                    await ApplyFilter();
                }
            }
            CssPropertyId = 0;
        }
        private async Task ApplyFilter()
        {
            if (FilteredCssPropertyDTO == null || CssPropertyDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                await LoadData();
                Title = $"All CSS Properties ({FilteredCssPropertyDTO.Count})";
            }
            else
            {
                if (lastSearchTerm != SearchTerm)
                {
                    await LoadData();
                }

            }
            lastSearchTerm = SearchTerm;
        }
        protected void SortCssProperty(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredCssPropertyDTO == null)
            {
                return;
            }
            if (sortColumn == "PropertyName")
            {
                FilteredCssPropertyDTO = FilteredCssPropertyDTO.OrderBy(v => v.PropertyName).ToList();
            }
            else if (sortColumn == "PropertyName Desc")
            {
                FilteredCssPropertyDTO = FilteredCssPropertyDTO.OrderByDescending(v => v.PropertyName).ToList();
            }
        }
        private async Task DeleteCssProperty(int Id)
        {
            //TODO Optionally remove child records here or warn about their existence
            var parameters = new ModalParameters();
            if (CssPropertyDataService != null)
            {
                var cssProperty = await CssPropertyDataService.GetCssPropertyById(Id);
                parameters.Add("Title", "Please Confirm, Delete CSS Property");
                parameters.Add("Message", $"PropertyName: {cssProperty?.PropertyName}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete CSS Property ({cssProperty?.PropertyName})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await CssPropertyDataService.DeleteCssProperty(Id);
                        ToastService?.ShowSuccess("CSS Property deleted successfully");
                        await LoadData();
                        await ApplyFilter();
                    }
                }
            }
            CssPropertyId = Id;
        }

        private async void EditCssProperty(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<CssPropertyAddEdit>("Edit CSS Property", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                    await ApplyFilter();
                }
            }
            CssPropertyId = Id;
        }

        private async Task OnValueChangedPageSize(int value)
        {
            pageSize = value;
            pageNumber = 1;
            await LoadData();
        }
        private async Task PageDown(bool goBeginning)
        {
            if (goBeginning || pageNumber <= 0)
            {
                pageNumber = 1;
            }
            else
            {
                pageNumber--;
            }
            await LoadData();
        }
        private async Task PageUp(bool goEnd)
        {
            int maximumPages = (int)Math.Ceiling((decimal)totalRows / pageSize);
            if (goEnd || pageNumber >= maximumPages)
            {
                pageNumber = maximumPages;
            }
            else
            {
                pageNumber++;
            }
            await LoadData();
        }

    }
}