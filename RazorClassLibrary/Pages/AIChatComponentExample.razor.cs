using OpenAI.Chat;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace RazorClassLibrary.Pages
{
    public partial class AIChatComponentExample : ComponentBase
    {
        string prompt = "What is the capital of the Peru?";
        string? history = "";
        System.ClientModel.ClientResult<ChatCompletion>? response;
        string? responseHistory;
        ChatClient openAI = new ChatClient("gpt-4o", Constants.OpenAIAPIKEY);
        private ElementReference inputElement;
        private ElementReference textAreaRefResponse;
        private ElementReference textAreaRefResponseHistory;
        private ElementReference textAreaRefPromptHistory;
        [Inject] public required IJSRuntime JSRuntime { get; set; }

        private async Task Chat()
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return;
            }
            response = await openAI.CompleteChatAsync(history, prompt);
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
        }
    }
}