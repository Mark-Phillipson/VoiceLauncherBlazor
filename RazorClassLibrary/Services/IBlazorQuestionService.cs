using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public interface IBlazorQuestionService
{
    Task<IReadOnlyList<QuizQuestion>> GetQuestionsAsync(int count, IEnumerable<BlazorQuestionType>? allowedTypes = null);
}
