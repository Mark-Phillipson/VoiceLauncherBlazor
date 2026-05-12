using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public interface IQuizService
{
    Task<IReadOnlyList<QuizQuestion>> GenerateQuestionsAsync(int count);

    List<QuizScore> DeserializeScores(string? json);

    string SerializeScores(IEnumerable<QuizScore> scores);
}