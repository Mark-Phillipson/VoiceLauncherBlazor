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

namespace RazorClassLibrary.Pages
{
    public partial class AIChatComponentExample : ComponentBase
    {
        [Inject] public required IPromptDataService PromptDataService { get; set; }
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        ChatHistory chatHistory = new();
        private bool isPluginImported = false;
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
        string predefinedPrompt = "";
        protected override async Task OnInitializedAsync()
        {
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
            prompts = await PromptDataService.GetAllPromptsAsync();
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
            processing = true;

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
            chatHistory.AddUserMessage(prompt);
            // response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            responseHistory.AddAssistantMessage(response.Content ?? "");
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
        private async Task Forget()
        {
            responseHistory.Clear();
            response.Items.Clear();
            selectedPrompt = null;
            addedPredefinedPrompt = false;
            chatHistory.Clear();
            await inputElement.FocusAsync();
        }
        private async Task OnValueChangedMethodName(int id)
        {
            selectedPrompt = await PromptDataService.GetPromptById(id);
            await inputElement.FocusAsync();
        }
        bool showHistory = false;
        bool processing = false;
        void ToggleHistory()
        {
            {
                showHistory = !showHistory;
            }
        }
        private async Task EnteredChat(KeyboardEventArgs e)
        {
            System.Console.WriteLine(e.Key);
            if (e.Key == "Enter")
            {
                // Handle the Enter key press event
                await ProcessChat();
            }
        }
        private async Task CopyItemAsync(string itemToCopy)
        {
            if (string.IsNullOrEmpty(itemToCopy)) { return; }
            await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopy);

        }
    }
}
public class MarkInformation
{
    [KernelFunction]
    public string GetInformationAboutMark(string dataCategory)
    {
        if (dataCategory == "age")
        {
            return "Mark Was Born on the a day in August 1964 so he is currently sixty years old";
        }
        else if (dataCategory == "location")
        {
            return "Mark's location is Maidstone Kent";
        }
        else if (dataCategory == "job")
        {
            return "Mark's is a .NET software developer freelancing on Upwork";
        }
        else if (dataCategory == "hobbies")
        {
            return "Mark's hobbies include riding the bicycle, reading and programming in C#";
        }
        else if (dataCategory == "surname")
        {
            return "Mark has a last name of Phillipson";
        }
        else
        {
            return "I'm sorry, I don't know that information about Mark.";
        }
    }

}