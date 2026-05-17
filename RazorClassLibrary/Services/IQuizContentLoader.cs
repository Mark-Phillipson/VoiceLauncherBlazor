using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

/// <summary>
/// Interface for loading quiz pack content from various sources.
/// Implementations can load from JSON files, databases, or remote APIs.
/// </summary>
public interface IQuizContentLoader
{
    /// <summary>
    /// Loads quiz pack metadata by pack ID.
    /// </summary>
    /// <param name="packId">Unique identifier for the pack (e.g., "cursorless", "talon").</param>
    /// <returns>QuizPack metadata if found; null otherwise.</returns>
    Task<QuizPack?> LoadPackMetadataAsync(string packId);

    /// <summary>
    /// Loads all available quiz facts for a given pack.
    /// Facts are the raw data used to generate quiz questions.
    /// </summary>
    /// <param name="packId">Unique identifier for the pack.</param>
    /// <returns>List of QuizFacts if pack is found; empty list otherwise.</returns>
    Task<IReadOnlyList<QuizFact>> LoadFactsAsync(string packId);

    /// <summary>
    /// Loads manually curated quiz questions for a given pack.
    /// Manual questions are hand-authored, complex, or edge-case questions.
    /// </summary>
    /// <param name="packId">Unique identifier for the pack.</param>
    /// <returns>List of QuizQuestions if file is found; empty list otherwise.</returns>
    Task<IReadOnlyList<QuizQuestion>> LoadManualQuestionsAsync(string packId);

    /// <summary>
    /// Discovers and lists all available quiz pack IDs.
    /// </summary>
    /// <returns>Collection of pack IDs that can be loaded.</returns>
    Task<IReadOnlyCollection<string>> DiscoverPacksAsync();
}

/// <summary>
/// Internal model representing a single fact/item in a quiz pack.
/// Used internally by QuizService to generate questions.
/// </summary>
public record QuizFact(
    string Id,
    string SpokenForm,
    string Meaning,
    string Category,
    string? YoutubeLink = null,
    string? Source = null)
{
    public bool IsSameFact(QuizFact other) =>
        string.Equals(SpokenForm, other.SpokenForm, StringComparison.OrdinalIgnoreCase)
        && string.Equals(Meaning, other.Meaning, StringComparison.OrdinalIgnoreCase)
        && string.Equals(Category, other.Category, StringComparison.OrdinalIgnoreCase);
}
