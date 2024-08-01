using OpenAI.Chat;
using System.Collections.Generic;
using DataAccessLibrary.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SampleApplication.Services;

namespace RazorClassLibrary.Pages
{
    public partial class AIChatComponentExample : ComponentBase
    {
        [Inject] public required IPromptDataService PromptDataService { get; set; }
        private List<PromptDTO> prompts;
        private PromptDTO? selectedPrompt = null;
        private int selectedPromptId = 0;
        string prompt = "";
        string? history = "";
        System.ClientModel.ClientResult<ChatCompletion>? response;
        string? responseHistory;
        ChatClient openAI = new ChatClient("gpt-4o", Constants.OpenAIAPIKEY);
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
            if (!string.IsNullOrWhiteSpace(selectedPrompt?.PromptText))
            {
                predefinedPrompt = selectedPrompt.PromptText;
            }
            response = await openAI.CompleteChatAsync(predefinedPrompt, history, prompt);
            processing = false;
            responseHistory = responseHistory + "\n" + response.Value.ToString();
            history = history + "\n" + prompt;
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
            response = null;
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