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

    // Optional documentation link related to the question (e.g. shapes docs)
    public string? DocLink { get; set; }

    // Optional question-level image (relative to web root), e.g. "images/cursorless-docs/bolt.svg"
    public string? Image { get; set; }

    // Optional per-choice image paths (aligned with Choices list). Null entries mean no image for that choice.
    public List<string?> ChoiceImagePaths { get; set; } = new();

    // Optional: related TalonVoiceCommand Id so UI can deep-link to the command detail
    public int? RelatedCommandId { get; set; }

    // Optional: related Talon script file name (e.g. "foot_switch_FS3.talon") for context-aware labels
    public string? RelatedFileName { get; set; }
}