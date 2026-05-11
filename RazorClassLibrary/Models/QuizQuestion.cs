using System.Collections.Generic;

namespace RazorClassLibrary.Models;

public sealed class QuizQuestion
{
    public string Prompt { get; set; } = string.Empty;

    public List<string> Choices { get; set; } = new();

    public string CorrectAnswer { get; set; } = string.Empty;

    public int CorrectIndex { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;
}