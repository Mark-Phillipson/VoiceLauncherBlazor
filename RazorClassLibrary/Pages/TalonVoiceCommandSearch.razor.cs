using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using DataAccessLibrary.Models;
using SmartComponents.LocalEmbeddings;
using System.Linq;

namespace RazorClassLibrary.Pages
{
    public partial class TalonVoiceCommandSearch : ComponentBase
    {
        public string SearchTerm { get; set; } = string.Empty;
        public List<TalonVoiceCommand> Results { get; set; } = new();
        public bool IsLoading { get; set; }
        public bool HasSearched { get; set; }
        public bool UseSemanticMatching { get; set; } = true;
        private int maxResults = 20;

        [Inject]
        public DataAccessLibrary.Services.TalonVoiceCommandDataService? TalonService { get; set; }

        protected override void OnInitialized()
        {
            Results = new List<TalonVoiceCommand>();
        }

        protected async Task OnSearch()
        {
            IsLoading = true;
            HasSearched = true;
            StateHasChanged();
            if (TalonService is not null)
            {
                var allCommands = await TalonService.SemanticSearchAsync(""); // get all
                if (UseSemanticMatching && !string.IsNullOrWhiteSpace(SearchTerm) && SearchTerm.Length > 2)
                {
                    var candidates = allCommands.Select(c => (Item: c, Text: c.Command + " " + c.Script)).ToList();
                    using var embedder = new LocalEmbedder();
                    var matchedResults = embedder.EmbedRange(candidates.Select(x => x.Text).ToList());
                    var results = LocalEmbedder.FindClosestWithScore(embedder.Embed(SearchTerm), matchedResults, maxResults: maxResults);
                    var resultItems = results.Select(r => r.Item).ToList();
                    var similarityScores = results.ToDictionary(r => r.Item, r => r.Similarity);
                    Results = candidates
                        .Where(c => resultItems.Contains(c.Text))
                        .OrderByDescending(c => similarityScores[c.Text])
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
    }
}
