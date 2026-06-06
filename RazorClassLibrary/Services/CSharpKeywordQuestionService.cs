using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public sealed class CSharpKeywordQuestionService : ICSharpQuestionService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CSharpKeywordQuestionService> _logger;
    private static readonly string KeywordsUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/";
    private readonly ConcurrentDictionary<string, string> _docLinkCache = new(StringComparer.OrdinalIgnoreCase);
    // Map of common C# keywords to a short usage description used for "what would you use this keyword for" questions.
    private static readonly Dictionary<string, string> _keywordUsages = new(StringComparer.OrdinalIgnoreCase)
    {
        { "bool", "Represent a Boolean true/false value" },
        { "int", "Store a 32-bit integer number" },
        { "string", "Hold a sequence of characters (text)" },
        { "class", "Define a reference type (class)" },
        { "struct", "Define a value type (struct)" },
        { "interface", "Declare a contract of members that types must implement" },
        { "enum", "Define a set of named constant values" },
        { "delegate", "Declare a type that references methods (function pointer)" },
        { "event", "Expose notifications that other code can subscribe to" },
        { "async", "Mark a method as asynchronous to allow await inside it" },
        { "await", "Pause execution until an asynchronous operation completes" },
        { "using", "Import a namespace or define a scope for IDisposable resources" },
        { "foreach", "Iterate over each element in a collection" },
        { "lock", "Ensure exclusive access to a block across threads" },
        { "yield", "Return elements one at a time from an iterator method" },
        { "new", "Create a new instance of a type or hide an inherited member" },
        { "ref", "Pass an argument by reference so the callee can modify it" },
        { "out", "Pass an argument by reference and return a value via the parameter" },
        { "params", "Allow a method to accept a variable number of arguments" },
        { "is", "Test whether an object is compatible with a given type" },
        { "as", "Attempt a type conversion and return null on failure" },
        { "try", "Begin a block that will catch exceptions" },
        { "catch", "Handle exceptions thrown in a preceding try block" },
        { "finally", "Run cleanup code whether or not an exception occurred" },
        { "throw", "Raise an exception" },
        { "static", "Declare a member that belongs to the type rather than instances" },
        { "const", "Declare a compile-time constant" },
        { "readonly", "Declare a field that can only be assigned during initialization or in a constructor" },
        { "virtual", "Allow a method or property to be overridden in derived types" },
        { "override", "Provide a new implementation of an inherited virtual member" },
        { "sealed", "Prevent a class from being inherited or prevent an override from being further overridden" }
    };
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CSharpKeywordQuestionService(IHttpClientFactory httpFactory, IWebHostEnvironment environment, ILogger<CSharpKeywordQuestionService> logger)
    {
        _httpFactory = httpFactory;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IReadOnlyList<QuizQuestion>> GetQuestionsAsync(int count, IEnumerable<CSharpQuestionType>? allowedTypes = null)
    {
        if (count <= 0) return Array.Empty<QuizQuestion>();

        var keywords = await FetchKeywordsAsync();
        if (keywords == null || keywords.Count == 0)
        {
            // fallback to local manual questions if remote fetch fails
            var local = await LoadLocalManualQuestionsAsync();
            return local.Where(q => string.Equals(q.Category, "CSharp", StringComparison.OrdinalIgnoreCase))
                        .Select(EnsureCSharpDocLink)
                        .Take(count)
                        .ToList();
        }

        var unique = keywords.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (unique.Count == 0)
            return Array.Empty<QuizQuestion>();

        var questions = new List<QuizQuestion>();
        // Normalize allowed types set
        var allowedSet = allowedTypes?.ToHashSet() ?? new HashSet<CSharpQuestionType> { CSharpQuestionType.All };

        // Precompute pools for usage/collection-focused choices when the caller requests them
        var usageKeywordPool = unique.Where(k => _keywordUsages.ContainsKey(k.ToLowerInvariant())).ToList();
        var collectionKeywordPool = usageKeywordPool.Where(k => IsCollectionUsage(_keywordUsages[k.ToLowerInvariant()])).ToList();

        try
        {
            _logger?.LogInformation("GetQuestionsAsync: count={Count}, allowed={Allowed}", count, string.Join(',', allowedSet));
            _logger?.LogInformation("Pools: unique={Unique}, usage={UsageCount}, collections={CollectionCount}, wantsAll={WantsAll}", unique.Count, usageKeywordPool.Count, collectionKeywordPool.Count, allowedSet.Contains(CSharpQuestionType.All));
            Console.WriteLine($"[CSharpQSvc] GetQuestionsAsync: count={count}, allowed={string.Join(',', allowedSet)}");
            Console.WriteLine($"[CSharpQSvc] Pools: unique={unique.Count}, usage={usageKeywordPool.Count}, collections={collectionKeywordPool.Count}, wantsAll={allowedSet.Contains(CSharpQuestionType.All)}");
        }
        catch { }

        bool wantsAll = allowedSet.Contains(CSharpQuestionType.All);
        bool wantsIdentificationOnly = !wantsAll && allowedSet.Contains(CSharpQuestionType.Identification) && !allowedSet.Contains(CSharpQuestionType.Usage) && !allowedSet.Contains(CSharpQuestionType.Collections);
        bool wantsUsageOnly = !wantsAll && allowedSet.Contains(CSharpQuestionType.Usage) && !allowedSet.Contains(CSharpQuestionType.Identification) && !allowedSet.Contains(CSharpQuestionType.Collections);
        bool wantsCollectionsOnly = !wantsAll && allowedSet.Contains(CSharpQuestionType.Collections) && !allowedSet.Contains(CSharpQuestionType.Identification) && !allowedSet.Contains(CSharpQuestionType.Usage);

        for (var i = 0; i < count; i++)
        {
            // Choose correct keyword from an appropriate pool depending on requested types
            var correctPool = unique;
            if (!allowedSet.Contains(CSharpQuestionType.All))
            {
                // If caller asked specifically for Usage questions, prefer keywords with usage descriptions
                if (wantsUsageOnly && usageKeywordPool.Count > 0)
                {
                    correctPool = usageKeywordPool;
                }
                else if (wantsCollectionsOnly)
                {
                    // Build an "effective" collection pool. If the strict collection pool is
                    // too small, broaden it with a small curated set of iteration-related
                    // keywords and with usage-based candidates that mention collection/iteration
                    // semantics. This synthesizes collection-style usage questions instead of
                    // forcing repeated picks from a tiny pool.
                    var fallbackCollectionCandidates = new[] { "for", "while", "do", "foreach", "yield" };
                    var extendedCollectionPool = collectionKeywordPool
                        .Union(unique.Where(k => fallbackCollectionCandidates.Contains(k, StringComparer.OrdinalIgnoreCase)))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var syntheticPool = usageKeywordPool
                        .Where(k => _keywordUsages.TryGetValue(k.ToLowerInvariant(), out var u) && IsCollectionUsage(u))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    // Prefer strict collection keywords, then curated iteration keywords, then any
                    // usage keywords that look collection-related. Finally fall back to the broader
                    // usage pool so we can synthesize sensible usage prompts.
                    if (extendedCollectionPool.Count > 0)
                        correctPool = extendedCollectionPool;
                    else if (syntheticPool.Count > 0)
                        correctPool = syntheticPool;
                    else if (usageKeywordPool.Count > 0)
                        correctPool = usageKeywordPool;
                    else
                        correctPool = unique; // last resort
                }
            }

            var correct = correctPool[Random.Shared.Next(correctPool.Count)];

            // Create a single HttpClient for potential doc-link resolution checks.
            var resolverClient = _httpFactory.CreateClient();
            resolverClient.DefaultRequestHeaders.UserAgent.ParseAdd("VoiceLauncherBlazor/1.0");

            var docLink = await ResolveDocLinkAsync(correct, resolverClient);

            // Randomly choose whether to create an identification (keyword) question
            // or a usage question ("what would you use this keyword for"). The
            // decision here respects the caller-specified allowedTypes parameter.
            var normalizedKey = (correct.StartsWith("@") ? correct.Substring(1) : correct).ToLowerInvariant();

            bool makeUsageQuestion;
            if (wantsIdentificationOnly)
            {
                makeUsageQuestion = false;
            }
            else if (wantsUsageOnly)
            {
                makeUsageQuestion = _keywordUsages.ContainsKey(normalizedKey);
            }
            else if (wantsCollectionsOnly)
            {
                // When Collections-only is requested we prefer usage-style prompts. If we
                // selected from an extended/synthetic pool above, allow usage questions
                // for any keyword that has a usage description. If a chosen keyword lacks a
                // prewritten usage, we'll still present a usage-style prompt using a
                // generic phrasing so the UI remains consistent.
                makeUsageQuestion = _keywordUsages.ContainsKey(normalizedKey) || usageKeywordPool.Contains(correct, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                // Default behaviour (wantsAll or mixed): behave like before with a probabilistic mix
                makeUsageQuestion = _keywordUsages.ContainsKey(normalizedKey) && Random.Shared.NextDouble() < 0.45;
            }

            try { _logger?.LogInformation("Selected keyword '{Keyword}' (normalized '{Normalized}') -> makeUsageQuestion decision incoming", correct, normalizedKey); } catch { }
            try { Console.WriteLine($"[CSharpQSvc] Selected keyword '{correct}' (normalized '{normalizedKey}') - makeUsageQuestion incoming"); } catch { }

            if (makeUsageQuestion)
            {
                var correctUsage = _keywordUsages[normalizedKey];
                // Gather other usage descriptions as distractors
                var otherUsages = _keywordUsages.Values.Where(v => !string.Equals(v, correctUsage, StringComparison.OrdinalIgnoreCase)).Distinct().ToList();
                var distractorUsages = new List<string>();

                // sample up to 3 other usage descriptions
                for (var j = 0; j < 3 && otherUsages.Count > 0; j++)
                {
                    var idx = Random.Shared.Next(otherUsages.Count);
                    distractorUsages.Add(otherUsages[idx]);
                    otherUsages.RemoveAt(idx);
                }

                // pad if necessary with generated negative-like descriptions
                var fallbackPhrases = new[] { "Declare a variable", "Define a method", "Control flow statement", "Handle exceptions", "Perform type conversion", "Declare a constant" };
                var padIndex = 0;
                while (distractorUsages.Count < 3)
                {
                    var cand = fallbackPhrases[padIndex++ % fallbackPhrases.Length];
                    if (string.Equals(cand, correctUsage, StringComparison.OrdinalIgnoreCase) || distractorUsages.Contains(cand)) continue;
                    distractorUsages.Add(cand + (padIndex > 1 ? padIndex.ToString() : string.Empty));
                }

                var choices = new List<string>(distractorUsages) { correctUsage };
                choices = choices.OrderBy(_ => Random.Shared.Next()).ToList();
                var correctIndex = choices.FindIndex(x => string.Equals(x, correctUsage, StringComparison.OrdinalIgnoreCase));

                questions.Add(new QuizQuestion
                {
                    Prompt = $"What would you use the C# keyword '{correct}' for?",
                    Choices = choices,
                    CorrectAnswer = correctUsage,
                    CorrectIndex = correctIndex,
                    Category = "CSharp",
                    Source = "Microsoft Learn",
                    DocLink = docLink
                });
                try { _logger?.LogInformation("Question #{Index}: keyword='{Keyword}', type=Usage, correctIndex={CorrectIndex}", i, normalizedKey, correctIndex); } catch { }
                try { Console.WriteLine($"[CSharpQSvc] Question #{i}: keyword='{normalizedKey}', type=Usage, correctIndex={correctIndex}"); } catch { }
            }
            else
            {
                // Identification question: which of the following is a C# keyword?
                // Generate distractors that are NOT C# keywords so only one choice is correct.
                var distractors = GenerateNonKeywordDistractors(unique, correct, 3);

                var choices = new List<string>(distractors) { correct };
                // Shuffle choices
                choices = choices.OrderBy(_ => Random.Shared.Next()).ToList();

                var correctIndex = choices.FindIndex(x => string.Equals(x, correct, StringComparison.OrdinalIgnoreCase));

                questions.Add(new QuizQuestion
                {
                    Prompt = "Which of the following is a C# keyword?",
                    Choices = choices,
                    CorrectAnswer = correct,
                    CorrectIndex = correctIndex,
                    Category = "CSharp",
                    Source = "Microsoft Learn",
                    DocLink = docLink
                });
                try { _logger?.LogInformation("Question #{Index}: keyword='{Keyword}', type=Identification, correctIndex={CorrectIndex}", i, normalizedKey, correctIndex); } catch { }
                try { Console.WriteLine($"[CSharpQSvc] Question #{i}: keyword='{normalizedKey}', type=Identification, correctIndex={correctIndex}"); } catch { }
            }
        }

        return questions;
    }

    private async Task<List<string>> FetchKeywordsAsync()
    {
        try
        {
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("VoiceLauncherBlazor/1.0");
            var html = await client.GetStringAsync(KeywordsUrl);
            if (string.IsNullOrWhiteSpace(html)) return new List<string>();

            var matches = Regex.Matches(html, "<code[^>]*>(.*?)</code>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var list = new List<string>();
            foreach (Match m in matches)
            {
                var inner = WebUtility.HtmlDecode(m.Groups[1].Value ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(inner)) continue;

                var parts = Regex.Split(inner, "[,|\\s]+").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                foreach (var p in parts)
                {
                    var token = p.Trim();
                    if (Regex.IsMatch(token, "^@?[a-zA-Z_][a-zA-Z0-9_]*$"))
                    {
                        var normalized = token.StartsWith("@") ? token.Substring(1) : token;
                        if (normalized.Length >= 1 && normalized.Length <= 20)
                            list.Add(normalized);
                    }
                }
            }

            return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
        catch (Exception ex)
        {
            try { _logger?.LogWarning(ex, "Failed to fetch C# keywords from docs"); } catch { }
            return new List<string>();
        }
    }

    private async Task<List<QuizQuestion>> LoadLocalManualQuestionsAsync()
    {
        try
        {
            var webRootPath = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? _environment.WebRootPath
                : Path.Combine(_environment.ContentRootPath, "wwwroot");
            var filePath = Path.Combine(webRootPath, "CursorlessManualQuestions.json");
            if (!File.Exists(filePath)) return new List<QuizQuestion>();
            var json = await File.ReadAllTextAsync(filePath);
            var items = JsonSerializer.Deserialize<List<QuizQuestion>>(json, JsonOptions) ?? new List<QuizQuestion>();
            return items;
        }
        catch (Exception ex)
        {
            try { _logger?.LogWarning(ex, "Failed to read local manual questions as fallback"); } catch { }
            return new List<QuizQuestion>();
        }
    }

    private async Task<string> ResolveDocLinkAsync(string keyword, HttpClient client)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword)) return KeywordsUrl;

            if (_docLinkCache.TryGetValue(keyword, out var cached))
                return cached;

            var normalized = (keyword.StartsWith("@") ? keyword.Substring(1) : keyword).ToLowerInvariant();
            var candidate = string.Concat(KeywordsUrl, Uri.EscapeDataString(normalized));

            // Try a HEAD request first to avoid downloading body
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Head, candidate);
                var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                if (resp.IsSuccessStatusCode)
                {
                    _docLinkCache[keyword] = candidate;
                    return candidate;
                }
            }
            catch { /* fall through to GET fallback or search */ }

            // If HEAD didn't work, try a lightweight GET of headers
            try
            {
                using var respGet = await client.GetAsync(candidate, HttpCompletionOption.ResponseHeadersRead);
                if (respGet.IsSuccessStatusCode)
                {
                    _docLinkCache[keyword] = candidate;
                    return candidate;
                }
            }
            catch { /* ignore and fallback to search */ }

            var search = $"https://learn.microsoft.com/en-us/search/?terms={Uri.EscapeDataString(keyword + " c#")}";
            _docLinkCache[keyword] = search;
            return search;
        }
        catch (Exception ex)
        {
            try { _logger?.LogDebug(ex, "Failed to resolve doc link for {Keyword}", keyword); } catch { }
            var search = $"https://learn.microsoft.com/en-us/search/?terms={Uri.EscapeDataString(keyword + " c#")}";
            _docLinkCache[keyword] = search;
            return search;
        }
    }

    private static QuizQuestion EnsureCSharpDocLink(QuizQuestion question)
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

        question.DocLink = $"https://learn.microsoft.com/en-us/search/?terms={Uri.EscapeDataString((seed ?? "csharp keyword") + " c#")}";
        return question;
    }

    private static List<string> GenerateNonKeywordDistractors(IReadOnlyList<string> keywords, string correct, int count)
    {
        var distractors = new List<string>(count);
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { correct };

        var basePool = keywords.Where(k => !string.Equals(k, correct, StringComparison.OrdinalIgnoreCase)).ToList();
        var suffixes = new[] { "s", "es", "ing", "ed", "er", "ly", "Val", "X", "Api", "Type" };
        var prefixes = new[] { "pre", "post", "my", "is", "has" };
        var fallbackWords = new[] { "integer", "number", "variable", "method", "function", "class", "struct", "module", "delegate", "handler", "result", "value" };

        var attempts = 0;
        while (distractors.Count < count && attempts < 500)
        {
            attempts++;
            string candidate;

            if (basePool.Count > 0 && Random.Shared.NextDouble() < 0.7)
            {
                var baseWord = basePool[Random.Shared.Next(basePool.Count)];
                switch (Random.Shared.Next(3))
                {
                    case 0:
                        candidate = baseWord + suffixes[Random.Shared.Next(suffixes.Length)];
                        break;
                    case 1:
                        candidate = prefixes[Random.Shared.Next(prefixes.Length)] + baseWord;
                        break;
                    default:
                        candidate = baseWord + "_" + Random.Shared.Next(1, 200).ToString();
                        break;
                }
            }
            else
            {
                candidate = fallbackWords[Random.Shared.Next(fallbackWords.Length)];
                if (keywords.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                    candidate += Random.Shared.Next(1, 200).ToString();
            }

            candidate = candidate.Trim();
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            if (used.Contains(candidate)) continue;
            if (keywords.Contains(candidate, StringComparer.OrdinalIgnoreCase)) continue;

            distractors.Add(candidate);
            used.Add(candidate);
        }

        // Pad if we couldn't generate enough
        var padIndex = 1;
        while (distractors.Count < count)
        {
            var pad = $"NotAKeyword{padIndex++}";
            if (used.Contains(pad)) continue;
            distractors.Add(pad);
            used.Add(pad);
        }

        return distractors;
    }

    private static bool IsCollectionUsage(string usage)
    {
        if (string.IsNullOrWhiteSpace(usage)) return false;
        usage = usage.ToLowerInvariant();
        return usage.Contains("collection") || usage.Contains("iterate") || usage.Contains("element") || usage.Contains("enumer") || usage.Contains("iterate");
    }
}
