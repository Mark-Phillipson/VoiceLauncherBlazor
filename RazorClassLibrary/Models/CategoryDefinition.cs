namespace RazorClassLibrary.Models;

/// <summary>
/// Represents a category within a quiz pack (e.g., "Actions", "Navigation", "Scopes").
/// Categories help organize and filter quiz questions.
/// </summary>
public class CategoryDefinition
{
    /// <summary>
    /// Unique identifier for the category within the pack (e.g., "actions", "navigation").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name for the category (e.g., "Actions", "Navigation Commands").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of what this category covers.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional tags for additional filtering or metadata (e.g., ["beginner", "intermediate"]).
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Optional icon or color identifier for UI display.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Optional count of questions in this category (populated during pack loading).
    /// </summary>
    public int? QuestionCount { get; set; }
}
