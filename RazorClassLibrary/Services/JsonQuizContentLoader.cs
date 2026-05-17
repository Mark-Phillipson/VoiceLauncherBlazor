using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

/// <summary>
/// Loads quiz pack content from JSON files stored in wwwroot/quiz-packs/{packId}/.
/// Reads directly from disk via IWebHostEnvironment (Blazor Server safe).
/// </summary>
public class JsonQuizContentLoader : IQuizContentLoader
{
    private readonly IWebHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<JsonQuizContentLoader> _logger;
    private IReadOnlyCollection<string>? _cachedPackIds;

    public JsonQuizContentLoader(IWebHostEnvironment environment, ILogger<JsonQuizContentLoader> logger)
    {
        _environment = environment;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    private string WebRoot => !string.IsNullOrWhiteSpace(_environment.WebRootPath)
        ? _environment.WebRootPath
        : Path.Combine(_environment.ContentRootPath, "wwwroot");

    private string PackFilePath(string packId, string suffix) =>
        Path.Combine(WebRoot, "quiz-packs", packId, $"{packId}-{suffix}.json");

    public async Task<QuizPack?> LoadPackMetadataAsync(string packId)
    {
        var path = PackFilePath(packId, "pack");
        if (!File.Exists(path))
        {
            _logger.LogWarning("Pack metadata not found: {Path}", path);
            return null;
        }
        try
        {
            var json = await File.ReadAllTextAsync(path);
            var pack = JsonSerializer.Deserialize<QuizPack>(json, _jsonOptions);
            _logger.LogInformation("Loaded pack metadata for '{PackId}': {Title}", packId, pack?.Title);
            return pack;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pack metadata from {Path}", path);
            return null;
        }
    }

    public async Task<IReadOnlyList<QuizFact>> LoadFactsAsync(string packId)
    {
        var path = PackFilePath(packId, "facts");
        if (!File.Exists(path))
        {
            _logger.LogWarning("Facts file not found: {Path}", path);
            return [];
        }
        try
        {
            var json = await File.ReadAllTextAsync(path);
            // Use a DTO that handles both "Category" and legacy "CursorlessType" field names
            var dtos = JsonSerializer.Deserialize<List<FactDto>>(json, _jsonOptions) ?? [];
            var facts = dtos
                .Where(d => !string.IsNullOrWhiteSpace(d.SpokenForm) && !string.IsNullOrWhiteSpace(d.Meaning))
                .Select(d => new QuizFact(
                    Id: d.Id?.ToString() ?? Guid.NewGuid().ToString(),
                    SpokenForm: d.SpokenForm!.Trim(),
                    Meaning: d.Meaning!.Trim(),
                    Category: !string.IsNullOrWhiteSpace(d.Category) ? d.Category.Trim()
                              : !string.IsNullOrWhiteSpace(d.CursorlessType) ? d.CursorlessType.Trim()
                              : "General",
                    YoutubeLink: d.YoutubeLink,
                    Source: d.Source ?? "json"))
                .ToList();
            _logger.LogInformation("Loaded {Count} facts for pack '{PackId}'", facts.Count, packId);
            return facts.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading facts from {Path}", path);
            return [];
        }
    }

    // DTO that accepts both "Category" and legacy "CursorlessType" field names
    private sealed class FactDto
    {
        public object? Id { get; set; }
        public string? SpokenForm { get; set; }
        public string? Meaning { get; set; }
        public string? Category { get; set; }
        public string? CursorlessType { get; set; }
        public string? YoutubeLink { get; set; }
        public string? Source { get; set; }
    }

    public async Task<IReadOnlyList<QuizQuestion>> LoadManualQuestionsAsync(string packId)
    {
        var path = PackFilePath(packId, "manual-questions");
        if (!File.Exists(path))
        {
            _logger.LogInformation("No manual questions file for pack '{PackId}'", packId);
            return [];
        }
        try
        {
            var json = await File.ReadAllTextAsync(path);
            var questions = JsonSerializer.Deserialize<List<QuizQuestion>>(json, _jsonOptions) ?? [];
            _logger.LogInformation("Loaded {Count} manual questions for pack '{PackId}'", questions.Count, packId);
            return questions.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading manual questions from {Path}", path);
            return [];
        }
    }

    public async Task<IReadOnlyCollection<string>> DiscoverPacksAsync()
    {
        if (_cachedPackIds != null)
            return _cachedPackIds;

        var knownPacks = new[] { "cursorless", "talon" };
        var discoveredPacks = new List<string>();

        foreach (var packId in knownPacks)
        {
            var metadata = await LoadPackMetadataAsync(packId);
            if (metadata != null)
                discoveredPacks.Add(packId);
        }

        _cachedPackIds = discoveredPacks.AsReadOnly();
        _logger.LogInformation("Discovered {Count} quiz packs", _cachedPackIds.Count);
        return _cachedPackIds;
    }

    public void ClearCache() => _cachedPackIds = null;
}
