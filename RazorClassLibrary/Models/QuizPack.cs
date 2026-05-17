namespace RazorClassLibrary.Models;

/// <summary>
/// Represents metadata for a quiz pack (e.g., Cursorless, Talon).
/// Defines the pack's identity, versioning, and available categories.
/// </summary>
public class QuizPack
{
    /// <summary>
    /// Unique identifier for the quiz pack (e.g., "cursorless", "talon").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable title for the quiz pack (e.g., "Cursorless Voice Commands").
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of the quiz pack and its purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Version of the quiz pack (e.g., "1.0", "1.0.1").
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// List of categories available in this pack (e.g., "Actions", "Scopes", "Navigation").
    /// </summary>
    public List<CategoryDefinition> Categories { get; set; } = new();

    /// <summary>
    /// Optional icon or image URL for the pack (for UI display).
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Optional tags for filtering or discovery (e.g., ["productivity", "accessibility"]).
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Optional URL or path for additional documentation about the pack.
    /// </summary>
    public string? DocumentationUrl { get; set; }

    /// <summary>
    /// Format hint for content loading (e.g., "json", "database").
    /// Defaults to "json" for JSON-based packs.
    /// </summary>
    public string ContentFormat { get; set; } = "json";
}
