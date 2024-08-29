using OpenAI.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Generic;
using DataAccessLibrary.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SampleApplication.Services;
using Microsoft.SemanticKernel;

namespace RazorClassLibrary.Pages
{
    public partial class AIChatComponentExample : ComponentBase
    {
        [Inject] public required IPromptDataService PromptDataService { get; set; }
        private List<PromptDTO> prompts = new List<PromptDTO>();
        private PromptDTO? selectedPrompt = null;
        private int selectedPromptId = 0;
        string prompt = "";
        string? history = "";
        bool addedPredefinedPrompt = false;
        Microsoft.SemanticKernel.ChatMessageContent response = new Microsoft.SemanticKernel.ChatMessageContent();
        Microsoft.SemanticKernel.Kernel kernel = new Microsoft.SemanticKernel.Kernel();
        string? responseHistory;
        IChatCompletionService chatService = new OpenAIChatCompletionService("gpt-4o-mini", Constants.OpenAIAPIKEY);
        private ElementReference inputElement;
        private ElementReference textAreaRefResponse;
        private ElementReference textAreaRefResponseHistory;
        private ElementReference textAreaRefPromptHistory;
        string predefinedPrompt = "";
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
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

        private async Task Chat()
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return;
            }
            processing = true;
            ChatHistory chatHistory = new();

            PromptExecutionSettings settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
            if (addedPredefinedPrompt == false)
            {
                if (!string.IsNullOrWhiteSpace(selectedPrompt?.PromptText))
                {
                    predefinedPrompt = selectedPrompt.PromptText;
                }
                addedPredefinedPrompt = true;
                chatHistory.AddUserMessage(predefinedPrompt);
                kernel.ImportPluginFromType<MarkInformation>();
            }
            chatHistory.AddUserMessage(prompt);
            response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            processing = false;
            responseHistory = responseHistory + "\n" + response.Items;
            try
            {
                await JSRuntime.InvokeVoidAsync("adjustTextArea", textAreaRefResponse);
                await JSRuntime.InvokeVoidAsync("adjustTextArea", textAreaRefPromptHistory);
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine(exception.Message);
            }
            try
            {
                await JSRuntime.InvokeVoidAsync("adjustTextArea", textAreaRefResponseHistory);
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine(exception.Message);
            }
            try
            {
                await JSRuntime.InvokeVoidAsync("adjustTextArea", textAreaRefPromptHistory);
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine(exception.Message);
            }
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
        private void ResizeResponse()
        {
            try
            {
                JSRuntime.InvokeVoidAsync("adjustTextArea", textAreaRefResponse);
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
        private void Forget()
        {
            history = "";
            responseHistory = "";
            response.Items.Clear();
            selectedPrompt = null;
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