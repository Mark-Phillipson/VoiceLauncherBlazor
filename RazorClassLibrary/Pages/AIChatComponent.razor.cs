using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Generic;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SampleApplication.Services;
using Microsoft.SemanticKernel;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Json;
namespace RazorClassLibrary.Pages;

public partial class AIChatComponent : ComponentBase, IDisposable
{
    [Parameter] public bool RunningInBlazorHybrid { get; set; } = false;
    private bool useCascadiaCodeFont = true;
    public bool DebounceEnabled
    {
        get => debounceEnabled;
        set
        {
            debounceEnabled = value;
            // Focus the textarea when debounce checkbox is changed
            _ = FocusInputElementAsync();
        }
    }
    private bool debounceEnabled = false;
    private string PromptInput
    {
        get => prompt;
        set
        {
            prompt = value;
            if (DebounceEnabled)
            {
                StartDebounceTimer();
            }            else
            {
                // If debounce is off, stop any running timers and countdown
                debounceTimer?.Stop();
                debounceTimer?.Dispose();
                debounceTimer = null;
                countdownTimer?.Stop();
                countdownTimer?.Dispose();
                countdownTimer = null;
                debounceCountdown = 0;
                debounceProgressPercentage = 0;
            }
        }
    }
    private async Task OnInputKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        // If debounce is enabled, we handle the Enter key here
        // Only send on Ctrl+Enter when debounce is disabled
        if (!DebounceEnabled && e.Key == "Enter" && e.CtrlKey)
        {
            await ProcessChat();
        }
    }    private int debounceCountdown = 0;
    private int debounceProgressPercentage = 0;
    private System.Timers.Timer? countdownTimer;[Inject] public required IPromptDataService PromptDataService { get; set; }
    [Inject] public required IQuickPromptDataService QuickPromptDataService { get; set; }
    [Inject] public required IJSRuntime JSRuntime { get; set; }
    [Inject] public required IConfiguration Configuration { get; set; } // Added
    [Inject] public required HttpClient Http { get; set; }

    ChatHistory chatHistory = new();
    private bool isPluginImported = false;
    private HashSet<object> expandedMessages = new HashSet<object>();
    private string OpenAIAPIKEY = "";

    // Model discovery & caching
    private DateTime modelsFetchedAt = DateTime.MinValue;
    private int ModelCacheMinutes = 60; // cache duration
    private string ModelFetchMessage = "";
    private bool isFetchingModels = false;
    private string customModelInput = "";
    // Filtering: only show chat/text models by default; user may toggle to show all
    private bool showAllModels = false;
    private List<string> RawFetchedModels = new List<string>();
    // Models that we will show to user (filtered based on heuristics)
    private List<string> DisplayedModels => (showAllModels ? RawFetchedModels : AvailableModels);
    // Models available for selection (can be configured via configuration key "OpenAI:Models" as comma-separated list)
    private List<string> AvailableModels = new List<string>();
    // Currently selected model
    private string SelectedModel = "o3-mini";
    private string TextBlock { get; set; } = "";
    private string AIComments { get; set; } = "";
    private int revertTo = 0;    ChatHistory responseHistory = new();
    private List<PromptDTO> prompts = new List<PromptDTO>();    private List<QuickPromptDTO> quickPrompts = new List<QuickPromptDTO>();
    private List<QuickPromptDTO> filteredQuickPrompts = new List<QuickPromptDTO>();
    private string quickPromptSearchTerm = "";
    private bool showQuickPrompts = true;
    private PromptDTO? selectedPrompt = null;
    private int selectedPromptId = 0;
    private string prompt = "";
    private string PromptWithDebounce
    {
        get => prompt;
        set
        {
            prompt = value;
            StartDebounceTimer();
        }
    }
    private System.Timers.Timer? debounceTimer;
    private int debounceMilliseconds = 4200; // Default 4.2 seconds
    public int DebounceMillisecondsInput
    {
        get => debounceMilliseconds;
        set
        {
            debounceMilliseconds = value;
            // Optionally, restart debounce timer if user changes value while typing
            if (debounceTimer != null)
            {
                StartDebounceTimer();
            }
        }
    }    private void StartDebounceTimer()
    {
        debounceTimer?.Stop();
        debounceTimer?.Dispose();
        countdownTimer?.Stop();
        countdownTimer?.Dispose();

        debounceCountdown = debounceMilliseconds;
        debounceProgressPercentage = 0;
        StateHasChanged();

        // Start countdown timer for visual feedback
        countdownTimer = new System.Timers.Timer(50); // update every 50ms
        countdownTimer.Elapsed += (s, e) =>
        {
            debounceCountdown -= 50;
            if (debounceCountdown < 0) debounceCountdown = 0;
            
            // Calculate progress percentage (0-100)
            debounceProgressPercentage = Math.Max(0, 100 - (int)((double)debounceCountdown / debounceMilliseconds * 100));
            
            InvokeAsync(StateHasChanged);
        };
        countdownTimer.AutoReset = true;
        countdownTimer.Start();

        debounceTimer = new System.Timers.Timer(debounceMilliseconds);        debounceTimer.Elapsed += async (_, __) =>
        {
            debounceTimer?.Stop();
            debounceTimer?.Dispose();
            debounceTimer = null;
            countdownTimer?.Stop();
            countdownTimer?.Dispose();
            countdownTimer = null;
            debounceCountdown = 0;
            debounceProgressPercentage = 0;
            await InvokeAsync(async () =>
            {
                await ProcessChat();
                StateHasChanged();
            });
        };

        debounceTimer.AutoReset = false;
        debounceTimer.Start();
    }
    // string? history = "";
    int historyCount = 0;
    bool addedPredefinedPrompt = false;
    Microsoft.SemanticKernel.ChatMessageContent response = new Microsoft.SemanticKernel.ChatMessageContent();    Microsoft.SemanticKernel.Kernel kernel = new Microsoft.SemanticKernel.Kernel(); 
    IChatCompletionService? chatService;
    private ElementReference inputElement;
    private ElementReference responseElement;
    string predefinedPrompt = "";
    private CancellationTokenSource? cancellationTokenSource; protected override async Task OnInitializedAsync()
    {
        // Try multiple configuration paths for the OpenAI API key
        OpenAIAPIKEY = Configuration["SmartComponents:ApiKey"] ??
                       Configuration["OpenAI:ApiKey"] ??
                       Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";

        if (string.IsNullOrWhiteSpace(OpenAIAPIKEY))
        {
            Message = "OpenAI API key not found. Please add it to appsettings.json under 'SmartComponents:ApiKey' or 'OpenAI:ApiKey', or set the OPENAI_API_KEY environment variable.";
            // Optionally, prevent further loading if the key is missing
            // return; 
        }
        else
        {
            Message = ""; // Clear any previous message if key is found
        }

        // Initialize defaults (will be overridden by API fetch when possible)
        var modelsConfig = Configuration["OpenAI:Models"] ?? Configuration["SmartComponents:Models"];
        if (!string.IsNullOrWhiteSpace(modelsConfig))
        {
            AvailableModels = modelsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        }
        else
        {
            AvailableModels = new List<string> { "o3-mini", "o2-mini", "o1-mini" };
        }

        // If we have an API key, try to fetch the latest list (cached)
        if (!string.IsNullOrWhiteSpace(OpenAIAPIKEY))
        {
            await FetchAvailableModelsAsync();
        }

        // Pick a configured model if provided, or default to preferred text-capable model
        var configuredModel = Configuration["OpenAI:Model"] ?? Configuration["SmartComponents:Model"] ?? Environment.GetEnvironmentVariable("OPENAI_MODEL");
        if (!string.IsNullOrWhiteSpace(configuredModel))
        {
            // If configured model is present and not already in list, add it so user can select it
            if (!AvailableModels.Contains(configuredModel) && !RawFetchedModels.Contains(configuredModel))
            {
                // keep custom-configured model available
                AvailableModels.Insert(0, configuredModel);
            }
            SelectedModel = configuredModel;
        }
        else
        {
            // prefer 'gpt-4o-mini' if available
            var preferred = AvailableModels.FirstOrDefault(s => s.Equals("gpt-4o-mini", StringComparison.OrdinalIgnoreCase))
                            ?? AvailableModels.FirstOrDefault(s => s.StartsWith("gpt-4o", StringComparison.OrdinalIgnoreCase))
                            ?? AvailableModels.FirstOrDefault(s => s.StartsWith("o3", StringComparison.OrdinalIgnoreCase))
                            ?? AvailableModels.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(preferred))
            {
                SelectedModel = preferred;
            }
        }

        await LoadData();
        if (inputElement.Id != null)
        {
            try
            {
                await inputElement.FocusAsync();
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine(exception.Message);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await FocusInputElementAsync();
        }
    }

    private async Task LoadData()
    {
        // Initialize chatService if the key was found
        if (!string.IsNullOrWhiteSpace(OpenAIAPIKEY))
        {
            // Use the currently selected model when creating the chat service
            chatService = new OpenAIChatCompletionService(SelectedModel, OpenAIAPIKEY);
        }
        else
        {
            // Handle the case where the key is missing, maybe disable chat functionality
            Message = "OpenAI API key is missing. Chat functionality disabled.";
            return; // Prevent loading prompts if service can't be initialized
        }

        prompts = await PromptDataService.GetAllPromptsAsync();
        quickPrompts = await QuickPromptDataService.GetAllQuickPromptsAsync(1, 1000, "");
        var prompt = prompts.Where(x => x.IsDefault).FirstOrDefault();
        if (prompt != null)
        {
            selectedPromptId = prompt.Id;
            selectedPrompt = await PromptDataService.GetPromptById(prompt.Id);
        }
    }
    private async Task ProcessChat()
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }
        // If the prompt is exactly 'testing', skip AI and just add to results
        if (prompt.Trim().Equals("testing", StringComparison.OrdinalIgnoreCase))
        {
            responseHistory.AddAssistantMessage("testing");
            revertTo = responseHistory.Count - 1;
            prompt = "";
            await inputElement.FocusAsync();
            processing = false;
            StateHasChanged();
            return;
        }

        Message = "";
        // Add a check if chatService failed to initialize due to missing key
        if (chatService == null)
        {
            Message = "OpenAI API key is missing or invalid. Cannot process chat.";
            return;
        }

        // Create a new cancellation token for this request
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = new CancellationTokenSource();

        prompts = await PromptDataService.GetAllPromptsAsync();
        processing = true;
        StateHasChanged();

        PromptExecutionSettings settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        if (addedPredefinedPrompt == false)
        {
            if (!string.IsNullOrWhiteSpace(selectedPrompt?.PromptText))
            {
                predefinedPrompt = selectedPrompt.PromptText;
            }
            addedPredefinedPrompt = true;
            chatHistory.AddSystemMessage(predefinedPrompt);
            if (isPluginImported == false)
            {
                // kernel.ImportPluginFromType<MarkInformation>();
                isPluginImported = true;
            }
        }
        if (selectedPrompt?.Description == "Do Dictation")
        {
            chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage($"{predefinedPrompt}\n The current value of the TextBlock is: {TextBlock}.\n" +
                $"The current value of the AIComments is: {AIComments}.\n" +
                $"The user has asked to do dictation. Please provide a response.\n");
        }
        chatHistory.AddUserMessage(prompt);
        // response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
        try
        {
            response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel, cancellationTokenSource.Token);
            if (selectedPrompt?.Description == "Do Dictation")
            {
                // Deserialize the JSON response
                var jsonResponse = response.Content ?? "";
                var chatResponse = System.Text.Json.JsonSerializer.Deserialize<ChatResponse>(jsonResponse);

                // Extract the values
                TextBlock = chatResponse?.TextBlock ?? "";
                AIComments = chatResponse?.Comments ?? "";
            }

        }
        catch (OperationCanceledException)
        {
            Message = "AI request was cancelled.";
            System.Console.WriteLine("AI request was cancelled by user.");
        }
        catch (System.Exception exception)
        {
            Message = "Error: " + exception.Message;
            System
            .Console.WriteLine(exception.Message);
        }

        // Only add response to history if not cancelled
        if (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            responseHistory.AddAssistantMessage(response.Content ?? "");
            revertTo = responseHistory.Count - 1;
        }

        prompt = "";
        await inputElement.FocusAsync();
        processing = false;
        StateHasChanged();
    }
    private void ResizeTextAreaPrompt()
    {
        try
        {
            JSRuntime.InvokeVoidAsync("adjustTextArea", inputElement);
        }
        catch (System.Exception exception)
        {
            System.Console.WriteLine(exception.Message);
        }
    }
    private async Task Clear()
    {
        prompt = "";
        await inputElement.FocusAsync();
    }

    private void CancelChat()
    {
        if (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
            Message = "Cancelling AI request...";
            StateHasChanged();
        }
    }
    private async Task Forget()
    {
        responseHistory.Clear();
        response.Items.Clear();
        if (selectedPrompt != null)
        {
            selectedPrompt.Description = null;
        }
        TextBlock = "";
        AIComments = "";
        selectedPrompt = null;
        addedPredefinedPrompt = false;
        chatHistory.Clear();
        await inputElement.FocusAsync();
        await LoadData();
    }
    private async Task OnValueChangedMethodName(int id)
    {
        await Forget();
        selectedPromptId = id;
        selectedPrompt = await PromptDataService.GetPromptById(id);
        StateHasChanged();
        await inputElement.FocusAsync();
    }

    // Called when the user changes the model selection from the UI
    private async Task ModelChanged(ChangeEventArgs e)
    {
        // SelectedModel is updated via binding; reinitialize service and clear conversation history
        // If the selected model appears to be a non-text model and we're not in showAllModels, warn and set message
        if (!showAllModels && !IsLikelyTextChatModel(SelectedModel))
        {
            Message = $"Warning: {SelectedModel} may not be text/chat-only. Toggle 'Show all' to include non-text models.";
        }
        else
        {
            Message = $"Model switched to {SelectedModel}";
        }

        // Reinitialize and clear
        await Forget();
        StateHasChanged();
        try { await inputElement.FocusAsync(); } catch { }
    }

    private async Task FetchAvailableModelsAsync(bool forceRefresh = false)
    {
        if (isFetchingModels) return;
        if (!string.IsNullOrWhiteSpace(OpenAIAPIKEY))
        {
            if (!forceRefresh && modelsFetchedAt != DateTime.MinValue && (DateTime.Now - modelsFetchedAt).TotalMinutes < ModelCacheMinutes && RawFetchedModels?.Count > 0)
            {
                ModelFetchMessage = $"Models last fetched {modelsFetchedAt:T}";
                return;
            }

            isFetchingModels = true;
            ModelFetchMessage = "Fetching models...";
            StateHasChanged();

            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, "https://api.openai.com/v1/models");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", OpenAIAPIKEY);
                var resp = await Http.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("data", out var data))
                    {
                        var list = data.EnumerateArray()
                            .Select(x => x.GetProperty("id").GetString())
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => s!)
                            .Distinct()
                            .OrderBy(s => s)
                            .ToList();

                        // keep a raw copy for 'show all' mode
                        RawFetchedModels = list.ToList();

                        // Filter to chat/text-only models by default
                        var filtered = list.Where(IsLikelyTextChatModel).ToList();

                        if (filtered.Count == 0)
                        {
                            // If filtering produced nothing, fall back to raw list but mark message
                            AvailableModels = list;
                            ModelFetchMessage = $"Fetched {list.Count} models (no text-only models detected)";
                        }
                        else
                        {
                            AvailableModels = filtered;
                            ModelFetchMessage = $"Fetched {filtered.Count} text-capable models (raw: {list.Count})";
                        }

                        // If preferred model (gpt-4o-mini) exists and nothing selected, prefer it
                        if ((string.IsNullOrWhiteSpace(SelectedModel) || !DisplayedModels.Contains(SelectedModel)) && AvailableModels.Count > 0)
                        {
                            var preferred = AvailableModels.FirstOrDefault(s => s.Equals("gpt-4o-mini", StringComparison.OrdinalIgnoreCase))
                                            ?? AvailableModels.FirstOrDefault(s => s.StartsWith("gpt-4o", StringComparison.OrdinalIgnoreCase))
                                            ?? AvailableModels.FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(preferred)) SelectedModel = preferred;
                        }

                        modelsFetchedAt = DateTime.Now;
                    }
                }
                else
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    ModelFetchMessage = $"Model fetch failed: {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    System.Console.WriteLine(err);
                }
            }
            catch (Exception ex)
            {
                ModelFetchMessage = $"Error fetching models: {ex.Message}";
            }
            finally
            {
                isFetchingModels = false;
                StateHasChanged();
            }
        }
        else
        {
            ModelFetchMessage = "No OpenAI API key; cannot fetch models.";
        }
    }

    private async Task RefreshModels()
    {
        await FetchAvailableModelsAsync(forceRefresh: true);

        // If the preferred model (gpt-4o-mini) is available after refresh, prefer it when nothing selected
        if (string.IsNullOrWhiteSpace(SelectedModel) || !DisplayedModels.Contains(SelectedModel))
        {
            var preferred = AvailableModels.FirstOrDefault(s => s.Equals("gpt-4o-mini", StringComparison.OrdinalIgnoreCase))
                            ?? AvailableModels.FirstOrDefault(s => s.StartsWith("gpt-4o", StringComparison.OrdinalIgnoreCase))
                            ?? AvailableModels.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(preferred)) SelectedModel = preferred;
        }
    }

    private async Task AddCustomModel()
    {
        if (string.IsNullOrWhiteSpace(customModelInput)) return;
        if (!AvailableModels.Contains(customModelInput))
        {
            AvailableModels.Insert(0, customModelInput);
        }
        if (!RawFetchedModels.Contains(customModelInput)) RawFetchedModels.Insert(0, customModelInput);
        SelectedModel = customModelInput;
        customModelInput = "";
        await ModelChanged(new ChangeEventArgs { Value = SelectedModel });
    }

    private void ToggleShowAllModels()
    {
        showAllModels = !showAllModels;
        ModelFetchMessage = showAllModels ? "Showing all raw models" : "Showing filtered text-capable models";
    }
    bool showHistory = false;
    bool processing = false;
    private string Message = "";

    void ToggleHistory()
    {
        {
            showHistory = !showHistory;
        }
    }
    // Enter key is no longer needed for chat submission
    // private async Task EnteredChat(KeyboardEventArgs e)
    // {
    //     if (e.Key == "Enter")
    //     {
    //         await ProcessChat();
    //     }
    // }
    private async Task CopyItemAsync(string? itemToCopy)
    {
        if (string.IsNullOrEmpty(itemToCopy)) { return; }
        await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopy);
    }
    private async Task CopyChatResultsAsync()
    {
        string contentToCopy = "";

        // Always use the currently displayed message (revertTo index)
        if (responseHistory?.Count > 0 && revertTo >= 0 && revertTo < responseHistory.Count)
        {
            var displayedMessage = responseHistory[revertTo]?.Content ?? "";
            // Try to extract TextBlock/AIComments if JSON
            try
            {
                var chatResponse = System.Text.Json.JsonSerializer.Deserialize<ChatResponse>(displayedMessage);
                if (!string.IsNullOrWhiteSpace(chatResponse?.TextBlock))
                {
                    contentToCopy = chatResponse.TextBlock;
                }
                else if (!string.IsNullOrWhiteSpace(chatResponse?.Comments))
                {
                    contentToCopy = chatResponse.Comments;
                }
                else
                {
                    contentToCopy = displayedMessage;
                }
            }
            catch
            {
                // Not JSON, just use the raw message
                contentToCopy = displayedMessage;
            }
        }
        // Last fallback: User's input (if nothing else is available)
        else if (!string.IsNullOrWhiteSpace(PromptInput))
        {
            contentToCopy = PromptInput;
        }

        if (!string.IsNullOrWhiteSpace(contentToCopy))
        {
            await CopyItemAsync(contentToCopy);

            // If running in Blazor Hybrid, also trigger paste functionality
            if (RunningInBlazorHybrid)
            {
                await TriggerPasteAsync();
            }
        }
    }
    private async Task TriggerPasteAsync()
    {
        try
        {
            Console.WriteLine("TriggerPasteAsync called from C#");
            // Use JavaScript to trigger paste functionality
            await JSRuntime.InvokeVoidAsync("blazorHybrid.triggerPaste");
            Console.WriteLine("JavaScript triggerPaste called successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Paste functionality error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Fallback: just copy to clipboard (already done above)
        }
    }
    private async Task FocusResponseElement()
    {
        if (responseElement.Id != null)
        {
            try
            {
                await responseElement.FocusAsync();
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine(exception.Message);
            }
        }
    }
    private async Task RevertPrevious()
    {
        revertTo--;
        if (revertTo < 0)
        {
            revertTo = 0;
        }
        await LoadHistory();
    }
    private async Task RevertNext()
    {
        revertTo++;
        if (revertTo >= responseHistory.Count)
        {
            revertTo = responseHistory.Count - 1;
        }
        await LoadHistory();
    }
    private async Task LoadHistory()
    {
        // Check if there are any messages in the history
        if (responseHistory?.LastOrDefault()?.Content != null && responseHistory.Count > 0)
        {

            // Get the latest message from the history
            var latestMessage = responseHistory?[revertTo];

            if (latestMessage != null)
            {
                // Deserialize the JSON response
                var jsonResponse = latestMessage.Content ?? "";
                var chatResponse = System.Text.Json.JsonSerializer.Deserialize<ChatResponse>(jsonResponse);

                // Extract the values
                TextBlock = chatResponse?.TextBlock ?? "";
                AIComments = chatResponse?.Comments ?? "";
                StateHasChanged();
                await inputElement.FocusAsync();

            }
        }
    }
    private void ToggleMessageExpansion(object message)
    {
        if (expandedMessages.Contains(message))
            expandedMessages.Remove(message);
        else
            expandedMessages.Add(message);
    }
    private void ToggleFont()
    {
        useCascadiaCodeFont = !useCascadiaCodeFont;
    }    public void Dispose()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        debounceTimer?.Stop();
        debounceTimer?.Dispose();
        countdownTimer?.Stop();
        countdownTimer?.Dispose();
    }

    private async Task ApplyQuickPrompt(QuickPromptDTO quickPrompt)
    {
        if (quickPrompt?.PromptText == null) return;

        // Get current content from TextBlock or latest response
        var currentContent = !string.IsNullOrWhiteSpace(TextBlock) 
            ? TextBlock 
            : responseHistory?.LastOrDefault()?.Content ?? "";

        // Apply the quick prompt to the current content
        var promptToSend = $"{quickPrompt.PromptText}\n\nCurrent content:\n{currentContent}";
        
        // Set the prompt and process it
        prompt = promptToSend;
        
        // Clear the quick prompt search
        quickPromptSearchTerm = "";
        filteredQuickPrompts.Clear();
        showQuickPrompts = false;
        
        // Process the chat with the applied quick prompt
        await ProcessChat();
        
        StateHasChanged();
    }

    private void ToggleQuickPromptPanel()
    {
        showQuickPrompts = !showQuickPrompts;
        if (!showQuickPrompts)
        {
            quickPromptSearchTerm = "";
            filteredQuickPrompts.Clear();
        }
        StateHasChanged();
    }

    private string GetQuickPromptTypeColorClass(string type)
    {
        return type?.ToUpper().Replace(" ", "-") switch
        {
            "FIXES" => "text-danger fw-bold",
            "FORMATTING" => "text-purple fw-bold", 
            "TEXT-GENERATION" => "text-success fw-bold",
            "FILE-CONVERSIONS" => "text-warning fw-bold",
            "CHECKERS" => "text-info fw-bold",
            "TRANSLATIONS" => "text-primary fw-bold",
            "CODE-GENERATION" => "text-primary fw-bold",
            "WRITING-HELPERS" => "text-danger fw-bold",
            _ => "text-muted"
        };
    }

    private void OnQuickPromptSearchInput(ChangeEventArgs e)
    {
        quickPromptSearchTerm = e.Value?.ToString() ?? "";
        FilterQuickPrompts();
        StateHasChanged();
    }

    private void FilterQuickPrompts()
    {
        if (quickPrompts == null)
        {
            filteredQuickPrompts = new List<QuickPromptDTO>();
            return;
        }

        if (string.IsNullOrWhiteSpace(quickPromptSearchTerm))
        {
            filteredQuickPrompts = new List<QuickPromptDTO>();
            return;
        }

        var searchTerm = quickPromptSearchTerm.ToLower().Trim();
        
        filteredQuickPrompts = quickPrompts
            .Where(q => q.IsActive && (
                (q.Command != null && q.Command.ToLower().Contains(searchTerm)) ||
                (q.Type != null && q.Type.ToLower().Contains(searchTerm)) ||
                (q.Description != null && q.Description.ToLower().Contains(searchTerm)) ||
                (q.PromptText != null && q.PromptText.ToLower().Contains(searchTerm))
            ))
            .OrderBy(q => q.Type).ThenBy(q => q.Command)
            .Take(10) // Limit to top 10 results for performance
            .ToList();
    }

    private async Task FocusInputElementAsync()
    {
        try
        {
            await inputElement.FocusAsync();
        }
        catch { }
    }

    // Heuristic: determine whether a model id is likely to be text/chat only (exclude image/audio/video models)
    private static readonly string[] ExcludeKeywords = new[] { "image", "vision", "whisper", "audio", "transcribe", "video", "dalle", "clip", "vision-", "speech" };

    private static bool IsLikelyTextChatModel(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId)) return false;
        var s = modelId.ToLowerInvariant();

        // If name contains any excluded keyword, treat as non-text model
        foreach (var k in ExcludeKeywords)
        {
            if (s.Contains(k)) return false;
        }

        // Common text/chat model prefixes
        if (s.StartsWith("gpt-") || s.StartsWith("gpt4") || s.StartsWith("gpt4o") || s.StartsWith("o3") || s.StartsWith("o2") || s.StartsWith("o1") || s.StartsWith("gpt5") || s.Contains("mini") || s.Contains("chat"))
        {
            return true;
        }

        // Fallback conservative: false
        return false;
    }
}