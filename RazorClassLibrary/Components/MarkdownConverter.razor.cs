using Markdig;
using Microsoft.AspNetCore.Components;
using System.IO;
using System.Threading.Tasks;

namespace RazorClassLibrary.Components
{
    public partial class MarkdownConverter : ComponentBase
    {
        // [Inject] public required IHttpClientFactory HttpClientFactory { get; set; }
        /// <summary>
        /// Optional: raw markdown content to render. If both MarkdownContent and FilePath are provided,
        /// FilePath takes precedence and the file will be read from the server filesystem.
        /// </summary>
        [Parameter] public string MarkdownContent { get; set; } = "# AI Chatbot\nThis is a **markdown** example.\n\n- Item 1\n- Item 2\n- Item 3";

        /// <summary>
        /// Optional: server-side path to a Markdown file. When set, the component will attempt to read the file
        /// and use its contents as the MarkdownContent. This runs on the server (Blazor Server / server-side code).
        /// Ensure the app has permission to read the target file and be careful with exposing arbitrary paths.
        /// </summary>
        [Parameter] public string? FilePath { get; set; }

        private MarkupString HtmlContent { get; set; }
        private bool isLoading = false;
        private string? errorMessage;

        protected override void OnInitialized()
        {
            // Initial render from provided MarkdownContent
            HtmlContent = (MarkupString)Markdown.ToHtml(MarkdownContent ?? string.Empty);
        }

    protected override async Task OnParametersSetAsync()
        {
            errorMessage = null;
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                // FilePath provided - attempt to read markdown from disk
                isLoading = true;
                try
                {
                    if (Path.IsPathRooted(FilePath) && File.Exists(FilePath))
                    {
                        var content = await File.ReadAllTextAsync(FilePath);
                        MarkdownContent = content ?? string.Empty;
                    }
                    else if (!Path.IsPathRooted(FilePath))
                    {
                        // Relative path: try relative to AppContext.BaseDirectory
                        var basePath = AppContext.BaseDirectory ?? string.Empty;
                        var combined = Path.Combine(basePath, FilePath);
                        if (File.Exists(combined))
                        {
                            var content = await File.ReadAllTextAsync(combined);
                            MarkdownContent = content ?? string.Empty;
                        }
                        else
                        {
                            errorMessage = $"Markdown file not found: {FilePath}";
                        }
                    }
                    else
                    {
                        errorMessage = $"Markdown file not found: {FilePath}";
                    }
                }
                catch (System.Exception ex)
                {
                    errorMessage = $"Error reading markdown file: {ex.Message}";
                }
                finally
                {
                    isLoading = false;
                }
            }

            // Convert whatever MarkdownContent is currently available to HTML
            // Use a Markdown pipeline that enables common table extensions and other helpful features
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions() // includes pipe tables, grid tables, footnotes, etc.
                .UsePipeTables()
                .UseGridTables()
                .Build();

            // Note: UseAdvancedExtensions already covers many table features; explicitly ensure pipe tables are available.
            // Pre-sanitize certain pipe sequences which can confuse table parsing when used inside cell content.
            // Replace unescaped pipes inside parentheses (e.g. `(seen | scene)`) with HTML entity, preserving table delimiters.
            var mdContent = MarkdownContent ?? string.Empty;
            try
            {
                // 1) Extract fenced code blocks (``` ... ```) to avoid replacing inside them
                    var fenceRegex = new System.Text.RegularExpressions.Regex(@"```[\s\S]*?```", System.Text.RegularExpressions.RegexOptions.Singleline);
                var fences = new System.Collections.Generic.List<string>();
                mdContent = fenceRegex.Replace(mdContent, match =>
                {
                    fences.Add(match.Value);
                    return $"{{FENCE_{fences.Count - 1}}}";
                });

                // 2) NOTE: blank-line-before-table insertion has been removed. The user requested manual fixes
                //    to markdown files instead of automatic newline insertion to avoid regressions.

                // 3) Replace '|' inside inline code spans (single backticks) with entity
                mdContent = System.Text.RegularExpressions.Regex.Replace(mdContent, @"`([^`]*?)`", match =>
                {
                    var inner = match.Groups[1].Value;
                    var replaced = inner.Replace("|", "&#124;");
                    return "`" + replaced + "`";
                });

                // 3) Replace '|' inside simple parentheses groups with '&#124;'
                mdContent = System.Text.RegularExpressions.Regex.Replace(mdContent, @"\(([^)]*\|[^)]*)\)", match =>
                {
                    var inner = match.Groups[1].Value;
                    var replaced = inner.Replace("|", "&#124;");
                    return "(" + replaced + ")";
                });

                // 4) Restore fenced code blocks
                for (int i = 0; i < fences.Count; i++)
                {
                    mdContent = mdContent.Replace($"{{FENCE_{i}}}", fences[i]);
                }
            }
            catch
            {
                // If regex processing fails, fall back to original content
            }

            // Then replace explicit escaped pipes (\|) with HTML entity as a final pass
            var sanitized = mdContent.Replace("\\|", "&#124;");

            // Protect against extremely deep/complex markdown (very large tables, pathological nesting)
            // by catching the Markdig ArgumentException about depth limits and falling back to a
            // simpler pipeline. Also defensively truncate excessively large inputs to avoid resource
            // exhaustion while still showing an informative message to the user.
            try
            {
                // If the input is extremely large, truncate and show a note instead of crashing the renderer.
                const int MaxRenderLength = 200_000; // characters
                var renderInput = sanitized;
                var wasTruncated = false;
                if (renderInput.Length > MaxRenderLength)
                {
                    renderInput = renderInput.Substring(0, MaxRenderLength);
                    wasTruncated = true;
                }

                HtmlContent = (MarkupString)Markdown.ToHtml(renderInput, pipeline);

                if (wasTruncated)
                {
                    errorMessage = "Markdown content was truncated for rendering because it was unusually large.";
                }
            }
            catch (System.ArgumentException ex) when (ex.Message?.Contains("depth", System.StringComparison.OrdinalIgnoreCase) == true)
            {
                // Likely "Markdown elements in the input are too deeply nested - depth limit exceeded" from Markdig.
                // Fall back to a more conservative pipeline (no advanced table parsing) which should avoid deep nesting.
                try
                {
                    var fallbackPipeline = new MarkdownPipelineBuilder().Build();
                    HtmlContent = (MarkupString)Markdown.ToHtml(sanitized, fallbackPipeline);
                    errorMessage = "Markdown was too deeply nested for the advanced renderer; rendered with a simpler fallback pipeline.";
                }
                catch
                {
                    // If even the fallback fails, safely show escaped raw markdown to the user instead of throwing.
                    var escaped = System.Net.WebUtility.HtmlEncode(sanitized);
                    HtmlContent = (MarkupString)$"<pre style=\"white-space:pre-wrap;\">{escaped}</pre>";
                    errorMessage = "Unable to render markdown due to extreme nesting; showing raw content.";
                }
            }
            catch (System.Exception ex)
            {
                // Generic fallback: show raw, escaped content and record an error message.
                var escaped = System.Net.WebUtility.HtmlEncode(sanitized);
                HtmlContent = (MarkupString)$"<pre style=\"white-space:pre-wrap;\">{escaped}</pre>";
                errorMessage = "Error rendering markdown: " + ex.Message;
            }
            await base.OnParametersSetAsync();
        }
    }
}