using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public interface ICSharpQuestionService
{
    /// <summary>
    /// Generate a set of quiz questions.
    /// If <paramref name="allowedTypes"/> is null or contains <see cref="CSharpQuestionType.All"/>, all question types are eligible.
    /// </summary>
    Task<IReadOnlyList<QuizQuestion>> GetQuestionsAsync(int count, IEnumerable<CSharpQuestionType>? allowedTypes = null);
}
