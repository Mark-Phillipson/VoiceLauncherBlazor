using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace TalonVoiceCommandsServer.Components.Pages
{
    public class TalonVoiceCommandAnalysisBase : ComponentBase
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected int CommandsCount { get; set; }
        protected int ListsCount { get; set; }
        protected int RepositoriesCount { get; set; }
        protected string LastRefresh { get; set; } = "Never";

        // Do not call JSInterop during prerender; wait for client-side runtime
        protected override Task OnInitializedAsync()
        {
            // initialize collections to avoid null refs during prerender
            Repositories = new List<RepoInfo>();
            TopApplications = new List<AppInfo>();
            return Task.CompletedTask;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // JS interop is safe now
                await Refresh();
            }
        }

        protected async Task Refresh()
        {
            try
            {
                // Call the existing JS helper to get storage info
                var storageInfo = await JS.InvokeAsync<StorageInfo?>("TalonStorageDB.getStorageInfo");
                if (storageInfo != null)
                {
                    CommandsCount = storageInfo.commands?.count ?? 0;
                    ListsCount = storageInfo.lists?.count ?? 0;
                }

                // Also attempt to get repositories via getFilterValues (returns repositories array)
                var filterValues = await JS.InvokeAsync<FilterValues?>("TalonStorageDB.getFilterValues");
                if (filterValues != null && filterValues.repositories != null)
                {
                    RepositoriesCount = filterValues.repositories.Length;
                }

                // Get breakdowns (repositories and top applications)
                try
                {
                    var breakdown = await JS.InvokeAsync<BreakdownResult?>("TalonStorageDB.getRepositoryAndApplicationBreakdown", 10);
                    if (breakdown != null)
                    {
                        // Map to local models with explicit null checks to satisfy the compiler
                        if (breakdown.repositories != null)
                        {
                            Repositories = breakdown.repositories.Select(r => new RepoInfo { Repository = r.repository ?? string.Empty, Count = r.count }).ToList();
                        }
                        else
                        {
                            Repositories = new List<RepoInfo>();
                        }

                        if (breakdown.applications != null)
                        {
                            TopApplications = breakdown.applications.Select(a => new AppInfo { Application = a.application ?? string.Empty, Count = a.count }).ToList();
                        }
                        else
                        {
                            TopApplications = new List<AppInfo>();
                        }
                    }
                }
                catch
                {
                    // Ignore breakdown failure, keep defaults
                }

                // Get lists breakdown (count and total list items)
                try
                {
                    var listsBreakdown = await JS.InvokeAsync<ListsBreakdown?>("TalonStorageDB.getListsBreakdown");
                    if (listsBreakdown != null)
                    {
                        ListsCount = listsBreakdown.listCount;
                        TotalListItems = listsBreakdown.totalItems;
                        PerList = listsBreakdown.perList?.Select(p => new PerListInfo { ListName = p.listName ?? string.Empty, ItemCount = p.itemCount }).ToList() ?? new List<PerListInfo>();
                    }
                }
                catch
                {
                    // ignore lists breakdown failure
                }

                LastRefresh = System.DateTime.Now.ToString("G");
            }
            catch
            {
                // JS interop failed - keep defaults
                CommandsCount = 0;
                ListsCount = 0;
                RepositoriesCount = 0;
                LastRefresh = "Error";
            }

            await InvokeAsync(StateHasChanged);
        }

        // Small DTOs for JS interop mapping
        protected class StorageInfo
        {
            public Meta? commands { get; set; }
            public Meta? lists { get; set; }
            public int totalSizeEstimate { get; set; }
        }

        protected class Meta
        {
            public int count { get; set; }
            public int sizeEstimate { get; set; }
            public string? lastUpdated { get; set; }
        }

        protected class FilterValues
        {
            public string[]? applications { get; set; }
            public string[]? repositories { get; set; }
            public string[]? modes { get; set; }
            public string[]? tags { get; set; }
            public string[]? titles { get; set; }
            public string[]? codeLanguages { get; set; }
            public string[]? operatingSystems { get; set; }
        }

        // Local models for UI
        protected class RepoInfo
        {
            public string Repository { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        protected class AppInfo
        {
            public string Application { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        protected class BreakdownResult
        {
            public JsPair[]? repositories { get; set; }
            public JsPair[]? applications { get; set; }
        }

        // Helper to map JS pairs
        protected class JsPair
        {
            public string? repository { get; set; }
            public int count { get; set; }
            public string? application { get; set; }
        }

    protected List<RepoInfo> Repositories { get; set; } = new List<RepoInfo>();
    protected List<AppInfo> TopApplications { get; set; } = new List<AppInfo>();
        
        // Lists breakdown properties
        protected int TotalListItems { get; set; }
        protected List<PerListInfo> PerList { get; set; } = new List<PerListInfo>();

        protected class PerListInfo
        {
            public string ListName { get; set; } = string.Empty;
            public int ItemCount { get; set; }
        }

        protected class ListsBreakdown
        {
            public int listCount { get; set; }
            public int totalItems { get; set; }
            public PerListPair[]? perList { get; set; }
        }

        protected class PerListPair
        {
            public string? listName { get; set; }
            public int itemCount { get; set; }
        }
    }
}
