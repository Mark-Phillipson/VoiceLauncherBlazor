namespace RazorClassLibrary.Models;

public sealed class AIReviewResult
{
    public int Index { get; set; }
    public bool Ok { get; set; }
    public string? SuggestedPrompt { get; set; }
    public string? Notes { get; set; }
    public string? OriginalPrompt { get; set; }
}
