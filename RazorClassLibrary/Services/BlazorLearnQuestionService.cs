using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public sealed class BlazorLearnQuestionService : IBlazorQuestionService
{
    private sealed record TopicEntry(string Token, string Url, BlazorQuestionType AreaType);

    private readonly IHttpClientFactory _httpFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<BlazorLearnQuestionService> _logger;

    private static readonly string[] LearnUrls =
    {
        "https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-9.0",
        "https://learn.microsoft.com/en-us/aspnet/core/blazor/data-binding/?view=aspnetcore-9.0",
        "https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/?view=aspnetcore-9.0",
        "https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-9.0",
        "https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-9.0"
    };

    private static readonly Dictionary<string, string> UsageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["@page"] = "Define a routable Blazor component endpoint",
        ["@layout"] = "Specify the layout component used by a page/component",
        ["@rendermode"] = "Control how a component is rendered and where it runs",
        ["@bind"] = "Create two-way binding between UI and component state",
        ["@onclick"] = "Attach a click event handler to an element",
        ["@inject"] = "Inject a service into a component",
        ["@typeparam"] = "Declare a generic type parameter for a component",
        ["[Parameter]"] = "Mark a component property as a public parameter",
        ["[CascadingParameter]"] = "Receive values provided by ancestor components",
        ["RenderFragment"] = "Represent UI content that can be rendered as a fragment",
        ["EventCallback"] = "Define a callback parameter from child to parent component",
        ["NavigationManager"] = "Read or change the current URI/navigation state",
        ["EditForm"] = "Build and validate form input in a Blazor component",
        ["InputText"] = "Bind and validate text input in forms",
        ["StateHasChanged"] = "Trigger component re-rendering",
        ["OnInitializedAsync"] = "Run async initialization logic when component starts",
        ["IJSRuntime"] = "Invoke JavaScript from Blazor"
    };

    private static readonly HashSet<string> KnownDirectives = new(StringComparer.OrdinalIgnoreCase)
    {
        "@page",
        "@layout",
        "@rendermode",
        "@bind",
        "@onclick",
        "@inject",
        "@typeparam",
        "@key",
        "@ref",
        "@attribute",
        "@code",
        "@using",
        "@implements",
        "@inherits",
        "@namespace"
    };

    private static readonly HashSet<string> KnownConceptTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "[Parameter]",
        "[CascadingParameter]",
        "RenderFragment",
        "RenderTreeBuilder",
        "EventCallback",
        "NavigationManager",
        "EditForm",
        "InputText",
        "InputNumber",
        "InputSelect",
        "InputDate",
        "InputCheckbox",
        "ComponentBase",
        "StateHasChanged",
        "OnInitializedAsync",
        "OnParametersSetAsync",
        "OnAfterRenderAsync",
        "IJSRuntime",
        "CascadingValue",
        "NavLink",
        "AuthorizeView",
        "LayoutView",
        "DynamicComponent"
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public BlazorLearnQuestionService(IHttpClientFactory httpFactory, IWebHostEnvironment environment, ILogger<BlazorLearnQuestionService> logger)
    {
        _httpFactory = httpFactory;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IReadOnlyList<QuizQuestion>> GetQuestionsAsync(int count, IEnumerable<BlazorQuestionType>? allowedTypes = null)
    {
        if (count <= 0)
        {
            return Array.Empty<QuizQuestion>();
        }

        var allowedSet = allowedTypes?.ToHashSet() ?? new HashSet<BlazorQuestionType> { BlazorQuestionType.All };
        var wantsAll = allowedSet.Contains(BlazorQuestionType.All);
        var wantsIdentificationOnly = !wantsAll
                                      && allowedSet.Contains(BlazorQuestionType.Identification)
                                      && !allowedSet.Contains(BlazorQuestionType.Usage);
        var wantsUsageOnly = !wantsAll
                             && allowedSet.Contains(BlazorQuestionType.Usage)
                             && !allowedSet.Contains(BlazorQuestionType.Identification);

        var topics = await FetchTopicsAsync();
        if (topics.Count == 0)
        {
            var fallback = await LoadLocalManualQuestionsAsync();
            return fallback
                .Where(q => string.Equals(q.Category, "Blazor", StringComparison.OrdinalIgnoreCase))
                .Select(EnsureBlazorDocLink)
                .Take(count)
                .ToList();
        }

        var scopedTopics = topics;
        if (!wantsAll)
        {
            var requestedAreas = allowedSet
                .Where(x => x != BlazorQuestionType.Identification && x != BlazorQuestionType.Usage)
                .ToHashSet();

            if (requestedAreas.Count > 0)
            {
                scopedTopics = topics
                    .Where(t => requestedAreas.Contains(t.AreaType))
                    .ToList();
            }
        }

        if (scopedTopics.Count == 0)
        {
            scopedTopics = topics;
        }

        var topicNames = scopedTopics.Select(t => t.Token).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var questions = new List<QuizQuestion>();

        for (var i = 0; i < count; i++)
        {
            var correctTopic = scopedTopics[Random.Shared.Next(scopedTopics.Count)];
            var correct = correctTopic.Token;
            var normalized = NormalizeToken(correct);
            var docLink = CreateBlazorSearchLink(correct, correctTopic.AreaType);

            var makeUsageQuestion = UsageMap.ContainsKey(normalized) && (wantsUsageOnly || (!wantsIdentificationOnly && Random.Shared.NextDouble() < 0.5));
            if (makeUsageQuestion)
            {
                var correctUsage = UsageMap[normalized];
                var otherUsages = UsageMap
                    .Where(kvp => !string.Equals(kvp.Key, normalized, StringComparison.OrdinalIgnoreCase))
                    .Select(kvp => kvp.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var distractors = new List<string>();
                while (distractors.Count < 3 && otherUsages.Count > 0)
                {
                    var idx = Random.Shared.Next(otherUsages.Count);
                    distractors.Add(otherUsages[idx]);
                    otherUsages.RemoveAt(idx);
                }

                var fallbacks = new[]
                {
                    "Define an HTTP API endpoint",
                    "Configure EF Core migrations",
                    "Run a background worker loop",
                    "Create a static CSS stylesheet"
                };

                var fallbackIndex = 0;
                while (distractors.Count < 3)
                {
                    var candidate = fallbacks[fallbackIndex++ % fallbacks.Length];
                    if (string.Equals(candidate, correctUsage, StringComparison.OrdinalIgnoreCase) || distractors.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    distractors.Add(candidate);
                }

                var choices = distractors.Append(correctUsage).OrderBy(_ => Random.Shared.Next()).ToList();
                var correctIndex = choices.FindIndex(x => string.Equals(x, correctUsage, StringComparison.OrdinalIgnoreCase));

                questions.Add(new QuizQuestion
                {
                    Prompt = $"What would you use '{correct}' for in Blazor?",
                    Choices = choices,
                    CorrectAnswer = correctUsage,
                    CorrectIndex = correctIndex,
                    Category = "Blazor",
                    Source = "Microsoft Learn",
                    DocLink = docLink
                });
            }
            else
            {
                var distractors = GenerateNonTopicDistractors(topicNames, correct, 3);
                var choices = distractors.Append(correct).OrderBy(_ => Random.Shared.Next()).ToList();
                var correctIndex = choices.FindIndex(x => string.Equals(x, correct, StringComparison.OrdinalIgnoreCase));

                questions.Add(new QuizQuestion
                {
                    Prompt = "Which of the following is a Blazor framework concept or directive?",
                    Choices = choices,
                    CorrectAnswer = correct,
                    CorrectIndex = correctIndex,
                    Category = "Blazor",
                    Source = "Microsoft Learn",
                    DocLink = docLink
                });
            }
        }

        return questions;
    }

    private async Task<List<TopicEntry>> FetchTopicsAsync()
    {
        var results = new Dictionary<string, TopicEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var url in LearnUrls)
        {
            var areaType = AreaTypeForUrl(url);
            try
            {
                var client = _httpFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("VoiceLauncherBlazor/1.0");
                var html = await client.GetStringAsync(url);
                if (string.IsNullOrWhiteSpace(html))
                {
                    continue;
                }

                var matches = Regex.Matches(html, "<code[^>]*>(.*?)</code>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match match in matches)
                {
                    var inner = WebUtility.HtmlDecode(match.Groups[1].Value ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(inner))
                    {
                        continue;
                    }

                    var parts = Regex.Split(inner, "[,|\\s]+")
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .ToArray();

                    foreach (var part in parts)
                    {
                        var token = NormalizeToken(part);
                        if (!IsBlazorToken(token))
                        {
                            continue;
                        }

                        var inferredType = InferAreaTypeFromToken(token, areaType);
                        if (!results.ContainsKey(token))
                        {
                            results[token] = new TopicEntry(token, url, inferredType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to fetch Blazor topics from {Url}", url);
            }
        }

        foreach (var known in UsageMap.Keys)
        {
            var normalized = NormalizeToken(known);
            if (!results.ContainsKey(normalized))
            {
                results[normalized] = new TopicEntry(normalized, "https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-9.0", BlazorQuestionType.Components);
            }
        }

        return results.Values.ToList();
    }

    private static BlazorQuestionType AreaTypeForUrl(string url)
    {
        if (url.Contains("routing", StringComparison.OrdinalIgnoreCase))
        {
            return BlazorQuestionType.Routing;
        }

        if (url.Contains("forms", StringComparison.OrdinalIgnoreCase) || url.Contains("data-binding", StringComparison.OrdinalIgnoreCase))
        {
            return BlazorQuestionType.FormsDataBinding;
        }

        if (url.Contains("javascript-interoperability", StringComparison.OrdinalIgnoreCase))
        {
            return BlazorQuestionType.JsInterop;
        }

        if (url.Contains("components", StringComparison.OrdinalIgnoreCase))
        {
            return BlazorQuestionType.Components;
        }

        return BlazorQuestionType.Components;
    }

    private static BlazorQuestionType InferAreaTypeFromToken(string token, BlazorQuestionType fallback)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return fallback;
        }

        if (token.Equals("OnInitializedAsync", StringComparison.OrdinalIgnoreCase)
            || token.Equals("OnParametersSetAsync", StringComparison.OrdinalIgnoreCase)
            || token.Equals("OnAfterRenderAsync", StringComparison.OrdinalIgnoreCase)
            || token.Equals("StateHasChanged", StringComparison.OrdinalIgnoreCase))
        {
            return BlazorQuestionType.Lifecycle;
        }

        return fallback;
    }

    private async Task<List<QuizQuestion>> LoadLocalManualQuestionsAsync()
    {
        try
        {
            var webRootPath = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? _environment.WebRootPath
                : Path.Combine(_environment.ContentRootPath, "wwwroot");
            var filePath = Path.Combine(webRootPath, "CursorlessManualQuestions.json");
            if (!File.Exists(filePath))
            {
                return new List<QuizQuestion>();
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<QuizQuestion>>(json, JsonOptions) ?? new List<QuizQuestion>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load local fallback questions for Blazor quiz");
            return new List<QuizQuestion>();
        }
    }

    private static string NormalizeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return string.Empty;
        }

        var cleaned = token.Trim();
        cleaned = cleaned.Trim('"', '\'', '`', ',', '.', ';', ':', '(', ')', '{', '}', '[', ']');
        cleaned = Regex.Replace(cleaned, "\\s+", " ");

        if (cleaned.StartsWith("@", StringComparison.Ordinal))
        {
            var atValue = "@" + cleaned.Substring(1).Trim();
            return atValue.ToLowerInvariant();
        }

        if (string.Equals(cleaned, "[Parameter]", StringComparison.OrdinalIgnoreCase) || string.Equals(cleaned, "[CascadingParameter]", StringComparison.OrdinalIgnoreCase))
        {
            return cleaned;
        }

        return cleaned;
    }

    private static bool IsBlazorToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if (token.Contains("=", StringComparison.Ordinal)
            || token.Contains("{", StringComparison.Ordinal)
            || token.Contains("}", StringComparison.Ordinal)
            || token.Length > 40)
        {
            return false;
        }

        if (token.StartsWith("@", StringComparison.Ordinal))
        {
            return KnownDirectives.Contains(token);
        }

        return UsageMap.ContainsKey(token)
               || KnownConceptTokens.Contains(token);
    }

    private static List<string> GenerateNonTopicDistractors(IReadOnlyCollection<string> allTopics, string correct, int count)
    {
        var distractors = new List<string>();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { correct };

        var seeds = new[] { "@route", "@render", "StateRefresh", "InputEmailAddress", "NavigateService", "ComponentRoot", "EventPipe", "BindOnce" };

        foreach (var seed in seeds.OrderBy(_ => Random.Shared.Next()))
        {
            if (distractors.Count >= count)
            {
                break;
            }

            if (used.Contains(seed) || allTopics.Contains(seed, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            distractors.Add(seed);
            used.Add(seed);
        }

        var pad = 1;
        while (distractors.Count < count)
        {
            var value = $"NotBlazorToken{pad++}";
            if (used.Contains(value))
            {
                continue;
            }

            distractors.Add(value);
            used.Add(value);
        }

        return distractors;
    }

    private static string CreateBlazorSearchLink(string token, BlazorQuestionType areaType)
    {
        var areaHint = areaType switch
        {
            BlazorQuestionType.Routing => "routing",
            BlazorQuestionType.FormsDataBinding => "forms data binding",
            BlazorQuestionType.Lifecycle => "lifecycle rendering",
            BlazorQuestionType.JsInterop => "javascript interop",
            _ => "components"
        };

        var term = $"aspnet core blazor {token} {areaHint}";
        return $"https://learn.microsoft.com/en-us/search/?terms={Uri.EscapeDataString(term)}";
    }

    private static QuizQuestion EnsureBlazorDocLink(QuizQuestion question)
    {
        if (question == null)
        {
            return new QuizQuestion();
        }

        if (!string.IsNullOrWhiteSpace(question.DocLink))
        {
            return question;
        }

        var seed = !string.IsNullOrWhiteSpace(question.CorrectAnswer)
            ? question.CorrectAnswer
            : question.Prompt;

        question.DocLink = $"https://learn.microsoft.com/en-us/search/?terms={Uri.EscapeDataString((seed ?? "blazor") + " blazor")}";
        return question;
    }
}
