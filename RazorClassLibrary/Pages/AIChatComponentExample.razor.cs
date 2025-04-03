//using OpenAI.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Generic;
using DataAccessLibrary.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SampleApplication.Services;
using Microsoft.SemanticKernel;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Json;
namespace RazorClassLibrary.Pages;

public partial class AIChatComponentExample : ComponentBase {
   [Inject] public required IPromptDataService PromptDataService { get; set; }
   [Inject] public required IJSRuntime JSRuntime { get; set; }
   ChatHistory chatHistory = new();
   private bool isPluginImported = false;
   private string TextBlock { get; set; } = "";
   private string AIComments { get; set; } = "";
   private int revertTo = 0;
   ChatHistory responseHistory = new();
   private List<PromptDTO> prompts = new List<PromptDTO>();
   private PromptDTO? selectedPrompt = null;
   private int selectedPromptId = 0;
   string prompt = "";
   // string? history = "";
   int historyCount = 0;
   bool addedPredefinedPrompt = false;
   Microsoft.SemanticKernel.ChatMessageContent response = new Microsoft.SemanticKernel.ChatMessageContent();
   Microsoft.SemanticKernel.Kernel kernel = new Microsoft.SemanticKernel.Kernel();
   IChatCompletionService chatService = new OpenAIChatCompletionService("gpt-4o-mini", Constants.OpenAIAPIKEY);
   private ElementReference inputElement;
   private ElementReference responseElement;
   string predefinedPrompt = "";
   protected override async Task OnInitializedAsync() {
      await LoadData();
      if (inputElement.Id != null) {
         try {
            await inputElement.FocusAsync();
         } catch (System.Exception exception) {
            System.Console.WriteLine(exception.Message);
         }
      }
   }

   private async Task LoadData() {
      prompts = await PromptDataService.GetAllPromptsAsync();
      var prompt = prompts.Where(x => x.IsDefault).FirstOrDefault();
      if (prompt != null) {
         selectedPromptId = prompt.Id;
         selectedPrompt = await PromptDataService.GetPromptById(prompt.Id);
      }
   }

   private async Task ProcessChat() {
      if (string.IsNullOrWhiteSpace(prompt)) {
         return;
      }
      processing = true;

      PromptExecutionSettings settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
      if (addedPredefinedPrompt == false) {
         if (!string.IsNullOrWhiteSpace(selectedPrompt?.PromptText)) {
            predefinedPrompt = selectedPrompt.PromptText;
         }
         addedPredefinedPrompt = true;
         chatHistory.AddSystemMessage(predefinedPrompt);
         if (isPluginImported == false) {
            // kernel.ImportPluginFromType<MarkInformation>();
            isPluginImported = true;
         }
      }
      chatHistory.AddUserMessage(prompt);
      // response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
      try {
         response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
         if (selectedPrompt?.Description == "Do Dictation") {
            // Deserialize the JSON response
            var jsonResponse = response.Content ?? "";
            var chatResponse = System.Text.Json.JsonSerializer.Deserialize<ChatResponse>(jsonResponse);

            // Extract the values
            TextBlock = chatResponse?.TextBlock ?? "";
            AIComments = chatResponse?.Comments ?? "";
         }

      } catch (System.Exception exception) {
         Message = "Error: " + exception.Message;
         System
         .Console.WriteLine(exception.Message);
      }
      responseHistory.AddAssistantMessage(response.Content ?? "");
      revertTo = responseHistory.Count - 1;
      prompt = "";
      await inputElement.FocusAsync();
      processing = false;
      StateHasChanged();
   }
   private void ResizeTextAreaPrompt() {
      try {
         JSRuntime.InvokeVoidAsync("adjustTextArea", inputElement);
      } catch (System.Exception exception) {
         System.Console.WriteLine(exception.Message);
      }
   }

   private async Task Clear() {
      prompt = "";
      await inputElement.FocusAsync();
   }
   private async Task Forget() {
      responseHistory.Clear();
      response.Items.Clear();
      if (selectedPrompt != null) {
         selectedPrompt.Description = null;
      }
      TextBlock = "";
      AIComments = "";
      selectedPrompt = null;
      addedPredefinedPrompt = false;
      chatHistory.Clear();
      await inputElement.FocusAsync();
   }
   private async Task OnValueChangedMethodName(int id) {
      await Forget();
      selectedPromptId = id;
      selectedPrompt = await PromptDataService.GetPromptById(id);
      StateHasChanged();
      await inputElement.FocusAsync();
   }
   bool showHistory = false;
   bool processing = false;
   private string Message = "";

   void ToggleHistory() {
      {
         showHistory = !showHistory;
      }
   }
   private async Task EnteredChat(KeyboardEventArgs e) {
      System.Console.WriteLine(e.Key);
      if (e.Key == "Enter") {
         // Handle the Enter key press event
         await ProcessChat();
      }
   }
   private async Task CopyItemAsync(string? itemToCopy) {
      if (string.IsNullOrEmpty(itemToCopy)) { return; }
      await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopy);

   }
   private async Task FocusResponseElement() {
      if (responseElement.Id != null) {
         try {
            await responseElement.FocusAsync();
         } catch (System.Exception exception) {
            System.Console.WriteLine(exception.Message);
         }
      }
   }
   private async Task RevertPrevious() {
      revertTo--;
      if (revertTo < 0) {
         revertTo = 0;
      }
      await LoadHistory();
   }
   private async Task RevertNext() {
      revertTo++;
      if (revertTo>= responseHistory.Count) {
         revertTo = responseHistory.Count - 1;
      }
      await LoadHistory();
   }
   private async Task LoadHistory() {
      // Check if there are any messages in the history
      if (responseHistory?.LastOrDefault()?.Content != null && responseHistory.Count > 0) {

         // Get the latest message from the history
         var latestMessage = responseHistory?[revertTo];

         if (latestMessage != null) {
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
}