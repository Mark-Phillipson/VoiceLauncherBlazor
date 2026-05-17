using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public interface IQuizService
{
    Task<IReadOnlyList<QuizQuestion>> GenerateQuestionsAsync(int count);
    Task<IReadOnlyList<QuizQuestion>> GenerateQuestionsAsync(int count, string packId, string? categoryFilter = null);

    List<QuizScore> DeserializeScores(string? json);

    string SerializeScores(IEnumerable<QuizScore> scores);
}