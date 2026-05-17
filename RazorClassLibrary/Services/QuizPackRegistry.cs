using Microsoft.Extensions.Logging;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

/// <summary>
/// Registry for discovering and managing available quiz packs.
/// Provides a single source of truth for pack availability and metadata.
/// </summary>
public class QuizPackRegistry
{
    private readonly IQuizContentLoader _contentLoader;
    private readonly ILogger<QuizPackRegistry> _logger;
    private Dictionary<string, QuizPack>? _packCache;

    public QuizPackRegistry(IQuizContentLoader contentLoader, ILogger<QuizPackRegistry> logger)
    {
        _contentLoader = contentLoader;
        _logger = logger;
    }

    /// <summary>
    /// Gets all available quiz packs.
    /// </summary>
    /// <returns>List of QuizPack metadata objects.</returns>
    public async Task<IReadOnlyList<QuizPack>> GetAllPacksAsync()
    {
        await EnsureCacheInitializedAsync();
        return _packCache?.Values.ToList().AsReadOnly() ?? [];
    }

    /// <summary>
    /// Gets a specific quiz pack by ID.
    /// </summary>
    /// <param name="packId">The pack identifier (e.g., "cursorless", "talon").</param>
    /// <returns>QuizPack if found; null otherwise.</returns>
    public async Task<QuizPack?> GetPackAsync(string packId)
    {
        await EnsureCacheInitializedAsync();
        return _packCache?.TryGetValue(packId, out var pack) == true ? pack : null;
    }

    /// <summary>
    /// Checks if a specific pack exists.
    /// </summary>
    public async Task<bool> PackExistsAsync(string packId)
    {
        return await GetPackAsync(packId) != null;
    }

    /// <summary>
    /// Gets a category by pack ID and category ID.
    /// </summary>
    public async Task<CategoryDefinition?> GetCategoryAsync(string packId, string categoryId)
    {
        var pack = await GetPackAsync(packId);
        return pack?.Categories.FirstOrDefault(c => c.Id == categoryId);
    }

    /// <summary>
    /// Clears the cache and forces a refresh on the next access.
    /// Useful if packs are added or updated at runtime.
    /// </summary>
    public void ClearCache()
    {
        _packCache = null;
        if (_contentLoader is JsonQuizContentLoader jsonLoader)
        {
            jsonLoader.ClearCache();
        }
    }

    private async Task EnsureCacheInitializedAsync()
    {
        if (_packCache != null)
            return;

        _packCache = new Dictionary<string, QuizPack>();
        var packIds = await _contentLoader.DiscoverPacksAsync();

        foreach (var packId in packIds)
        {
            try
            {
                var metadata = await _contentLoader.LoadPackMetadataAsync(packId);
                if (metadata != null)
                {
                    _packCache[packId] = metadata;
                    _logger.LogInformation($"Registered pack: {metadata.Title} ({packId})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading pack '{packId}': {ex.Message}");
            }
        }
    }
}
