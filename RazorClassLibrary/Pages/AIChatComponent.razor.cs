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
    public bool DebounceEnabled { get; set; } = true;
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
        if (!DebounceEnabled && e.Key == "Enter")
        {
            await ProcessChat();
        }
    }    private int debounceCountdown = 0;
    private int debounceProgressPercentage = 0;
    private System.Timers.Timer? countdownTimer;[Inject] public required IPromptDataService PromptDataService { get; set; }
    [Inject] public required IQuickPromptDataService QuickPromptDataService { get; set; }
    [Inject] public required IJSRuntime JSRuntime { get; set; }
    [Inject] public required IConfiguration Configuration { get; set; } // Added
    ChatHistory chatHistory = new();
    private bool isPluginImported = false;
    private HashSet<object> expandedMessages = new HashSet<object>();
    private string OpenAIAPIKEY = "";
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
    Microsoft.SemanticKernel.ChatMessageContent response = new Microsoft.SemanticKernel.ChatMessageContent();
    Microsoft.SemanticKernel.Kernel kernel = new Microsoft.SemanticKernel.Kernel(); private string model = "o3-mini";
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

    private async Task LoadData()
    {
        // Initialize chatService if the key was found
        if (!string.IsNullOrWhiteSpace(OpenAIAPIKEY))
        {
            chatService = new OpenAIChatCompletionService("o3-mini", OpenAIAPIKEY);
        }
        else
        {
            // Handle the case where the key is missing, maybe disable chat functionality
            Message = "OpenAI API key is missing. Chat functionality disabled.";
            return; // Prevent loading prompts if service can't be initialized
        }        prompts = await PromptDataService.GetAllPromptsAsync();
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

        // Prioritize AI response content (TextBlock) first
        if (!string.IsNullOrWhiteSpace(TextBlock))
        {
            contentToCopy = TextBlock;
        }
        // Second priority: AI Comments
        else if (!string.IsNullOrWhiteSpace(AIComments))
        {
            contentToCopy = AIComments;
        }
        // Third priority: Latest markdown response from history
        else if (responseHistory?.Count > 0 && !string.IsNullOrWhiteSpace(responseHistory.LastOrDefault()?.Content))
        {
            contentToCopy = responseHistory.LastOrDefault()?.Content ?? "";
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
}