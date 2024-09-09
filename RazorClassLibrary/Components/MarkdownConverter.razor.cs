using Markdig;
using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Components
{
    public partial class MarkdownConverter : ComponentBase
    {
        // [Inject] public required IHttpClientFactory HttpClientFactory { get; set; }
        [Parameter] public string MarkdownContent { get; set; } = "# AI Chatbot\nThis is a **markdown** example.\n\n- Item 1\n- Item 2\n- Item 3";
        private MarkupString HtmlContent { get; set; }

        protected override void OnInitialized()
        {
            HtmlContent = (MarkupString)Markdown.ToHtml(MarkdownContent);
        }
        protected override void OnParametersSet()
        {
            HtmlContent = (MarkupString)Markdown.ToHtml(MarkdownContent);
        }
    }
}