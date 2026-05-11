namespace RazorClassLibrary.Models;

public sealed class QuizScore
{
    public DateTime PlayedAt { get; set; }

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }
}