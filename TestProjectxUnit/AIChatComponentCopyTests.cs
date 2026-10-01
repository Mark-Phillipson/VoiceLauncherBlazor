using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Bunit;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using SampleApplication.Services;
using Xunit;

namespace TestProjectxUnit;

public class AIChatComponentCopyTests : BunitContext
{
    public AIChatComponentCopyTests()
    {
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection().Build());
        Services.AddSingleton<IPromptDataService>(new TestPromptDataService());
        Services.AddSingleton<IQuickPromptDataService>(new TestQuickPromptDataService());
        Services.AddHttpClient();
    }

    [Fact]
    public void Textarea_Input_BindsEditedTextToTextBlock()
    {
        var cut = Render<RazorClassLibrary.Pages.AIChatComponent>();

        SetPrivateValue(cut.Instance, "TextBlock", "Original response");
        SetPrivateValue(cut.Instance, "selectedPrompt", new PromptDTO { Description = "Do Dictation" });

        var history = new ChatHistory();
        history.AddAssistantMessage("Original response");
        SetPrivateValue(cut.Instance, "responseHistory", history);

        cut.Render();

        var textarea = cut.Find("textarea#responseElement");
        textarea.Input("Edited response text");

        Assert.Equal("Edited response text", GetTextBlock(cut.Instance));
    }

    private static string GetTextBlock(RazorClassLibrary.Pages.AIChatComponent component)
    {
        var prop = typeof(RazorClassLibrary.Pages.AIChatComponent).GetProperty("TextBlock", BindingFlags.Instance | BindingFlags.NonPublic);
        return (string?)prop?.GetValue(component) ?? string.Empty;
    }

    private static void SetPrivateValue(object target, string name, object? value)
    {
        var prop = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop != null)
        {
            prop.SetValue(target, value);
            return;
        }

        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        field?.SetValue(target, value);
    }

    private sealed class TestPromptDataService : IPromptDataService
    {
        public Task<List<PromptDTO>> GetAllPromptsAsync() => Task.FromResult(new List<PromptDTO>());
        public Task<List<PromptDTO>> SearchPromptsAsync(string serverSearchTerm) => Task.FromResult(new List<PromptDTO>());
        public Task<PromptDTO?> AddPrompt(PromptDTO promptDTO) => Task.FromResult<PromptDTO?>(promptDTO);
        public Task<PromptDTO?> GetPromptById(int Id) => Task.FromResult<PromptDTO?>(null);
        public Task<PromptDTO?> UpdatePrompt(PromptDTO promptDTO, string username) => Task.FromResult<PromptDTO?>(promptDTO);
        public Task DeletePrompt(int Id) => Task.CompletedTask;
    }

    private sealed class TestQuickPromptDataService : IQuickPromptDataService
    {
        public Task<List<QuickPromptDTO>> GetAllQuickPromptsAsync(int pageNumber, int pageSize, string serverSearchTerm) => Task.FromResult(new List<QuickPromptDTO>());
        public Task<List<QuickPromptDTO>> SearchQuickPromptsAsync(string serverSearchTerm) => Task.FromResult(new List<QuickPromptDTO>());
        public Task<QuickPromptDTO?> AddQuickPrompt(QuickPromptDTO quickPromptDTO) => Task.FromResult<QuickPromptDTO?>(quickPromptDTO);
        public Task<QuickPromptDTO?> GetQuickPromptById(int Id) => Task.FromResult<QuickPromptDTO?>(null);
        public Task<QuickPromptDTO?> UpdateQuickPrompt(QuickPromptDTO quickPromptDTO, string? username) => Task.FromResult<QuickPromptDTO?>(quickPromptDTO);
        public Task DeleteQuickPrompt(int Id) => Task.CompletedTask;
        public Task<int> GetTotalCount() => Task.FromResult(0);
        public Task<List<QuickPromptDTO>> GetQuickPromptsByType(string type) => Task.FromResult(new List<QuickPromptDTO>());
        public Task<QuickPromptDTO?> GetQuickPromptByCommand(string command) => Task.FromResult<QuickPromptDTO?>(null);
        public Task<List<string>> GetQuickPromptTypes() => Task.FromResult(new List<string>());
    }
}
