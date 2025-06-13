using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using DataAccessLibrary.Models;

namespace RazorClassLibrary.Pages
{
    public partial class TalonVoiceCommandSearch : ComponentBase
    {
        public string SearchTerm { get; set; } = string.Empty;
        public List<TalonVoiceCommand> Results { get; set; } = new();
        public bool IsLoading { get; set; }
        public bool HasSearched { get; set; }

        protected override void OnInitialized()
        {
            Results = new List<TalonVoiceCommand>();
        }

        protected async Task OnSearch()
        {
            IsLoading = true;
            HasSearched = true;
            StateHasChanged();
            Results = await TalonService.SemanticSearchAsync(SearchTerm);
            IsLoading = false;
            StateHasChanged();
        }
    }
}
