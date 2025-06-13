using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using DataAccessLibrary.Models;
using SmartComponents.LocalEmbeddings;
using System.Linq;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace RazorClassLibrary.Pages
{
    public partial class TalonVoiceCommandSearch : ComponentBase
    {
        private ElementReference searchInput;
        
        public string SearchTerm { get; set; } = string.Empty;
        public List<TalonVoiceCommand> Results { get; set; } = new();
        public bool IsLoading { get; set; }
        public bool HasSearched { get; set; }
        public bool UseSemanticMatching { get; set; } = true;
        private int maxResults = 20;

        [Inject]
        public DataAccessLibrary.Services.TalonVoiceCommandDataService? TalonService { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        protected override void OnInitialized()
        {
            Results = new List<TalonVoiceCommand>();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await searchInput.FocusAsync();
            }
        }        protected async Task OnSearch()
        {
            IsLoading = true;
            HasSearched = true;
            StateHasChanged();
            if (TalonService is not null)
            {
                var allCommands = await TalonService.SemanticSearchAsync(""); // get all
                if (UseSemanticMatching && !string.IsNullOrWhiteSpace(SearchTerm) && SearchTerm.Length > 2)
                {
                    var candidates = allCommands.Select((c, index) => (Item: c, Text: c.Command + " " + c.Script, Index: index)).ToList();
                    using var embedder = new LocalEmbedder();
                    var matchedResults = embedder.EmbedRange(candidates.Select(x => x.Text).ToList());
                    var results = LocalEmbedder.FindClosestWithScore(embedder.Embed(SearchTerm), matchedResults, maxResults: maxResults);
                    
                    // Create a lookup that can handle duplicate keys
                    var scoreLookup = results
                        .GroupBy(r => r.Item)
                        .ToDictionary(g => g.Key, g => g.Max(x => x.Similarity));
                    
                    var resultTexts = results.Select(r => r.Item).ToHashSet();
                    
                    Results = candidates
                        .Where(c => resultTexts.Contains(c.Text))
                        .OrderByDescending(c => scoreLookup.GetValueOrDefault(c.Text, 0))
                        .Select(c => c.Item)
                        .ToList();
                }
                else
                {
                    Results = await TalonService.SemanticSearchAsync(SearchTerm);
                }
            }
            else
            {
                Results = new List<TalonVoiceCommand>();
            }
            IsLoading = false;
            StateHasChanged();
        }

        public async Task OpenFileInVSCode(string filePath)
        {
            if (JSRuntime != null && !string.IsNullOrWhiteSpace(filePath))
            {
                // This will attempt to open the file in VS Code using the vscode://file URI scheme
                var uri = $"vscode://file/{filePath.Replace("\\", "/")}";
                await JSRuntime.InvokeVoidAsync("window.open", uri, "_blank");
            }
        }

        public async Task OnFilePathClick(MouseEventArgs e, string filePath)
        {
            // PreventDefault is not available in Blazor, but using @onclick on <a href="#"> avoids navigation
            await OpenFileInVSCode(filePath);
        }        public string GetFileName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;
            
            return System.IO.Path.GetFileName(filePath);
        }
    }
}
