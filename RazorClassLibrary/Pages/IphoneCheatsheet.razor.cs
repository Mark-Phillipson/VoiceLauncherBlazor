using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RazorClassLibrary.Pages;

public partial class IphoneCheatsheet : ComponentBase
{
    private static readonly IReadOnlyList<VoiceAccessCommand> AllCommands = BuildCommands();
    private readonly List<CommandGroupViewModel> filteredGroups = new();

    private string searchTerm = string.Empty;

    protected IReadOnlyList<CommandGroupViewModel> FilteredGroups => filteredGroups;

    protected int FilteredCount { get; private set; }

    protected string SearchTerm
    {
        get => searchTerm;
        set
        {
            searchTerm = value ?? string.Empty;
            ApplyFilter();
        }
    }

    protected override void OnInitialized()
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var term = SearchTerm.Trim();
        var hasTerm = !string.IsNullOrWhiteSpace(term);

        var query = hasTerm
            ? AllCommands.Where(c => c.Section.Contains(term, StringComparison.OrdinalIgnoreCase)
                                     || c.Phrase.Contains(term, StringComparison.OrdinalIgnoreCase)
                                     || c.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
            : AllCommands;

        var grouped = query
            .GroupBy(c => c.Section)
            .OrderBy(g => g.Key)
            .Select(g => new CommandGroupViewModel(g.Key, g.OrderBy(x => x.Phrase).ToList()))
            .ToList();

        filteredGroups.Clear();
        filteredGroups.AddRange(grouped);
        FilteredCount = query.Count();
    }

    private static List<VoiceAccessCommand> BuildCommands()
    {
        return new List<VoiceAccessCommand>
        {
            // Navigation
            new("Navigation","", "Go home", "Go to the home screen."),
            new("Navigation","", "Open [app]", "Open the named app."),
            new("Navigation","", "Go back", "Navigate back."),

            // System
            new("System","", "Turn volume up", "Increase device volume."),
            new("System","", "Turn volume down", "Decrease device volume."),
            new("System","", "Take screenshot", "Capture the screen."),

            // Interaction
            new("Interaction","", "Tap [item name]", "Tap the named item on screen."),
            new("Interaction","", "Show numbers", "Show numbers overlay for touch targets."),
            new("Interaction","", "Tap [number]", "Tap an item by its number."),

            // Scrolling & Gestures
            new("Scrolling & Gestures","", "Scroll up", "Scroll the page up."),
            new("Scrolling & Gestures","", "Scroll down", "Scroll the page down."),
            new("Scrolling & Gestures","", "Swipe left", "Swipe left gesture."),
            new("Scrolling & Gestures","", "Zoom in", "Zoom in on content."),

            // Text Input
            new("Text Input","", "Tap [text field]", "Focus the text field."),
            new("Text Input","", "Dictate text", "Start dictation to enter text."),
            new("Text Input","", "New line", "Insert a newline."),

            // Editing
            new("Editing","", "Delete that", "Delete the selected content."),
            new("Editing","", "Delete last word", "Remove the previous word."),
            new("Editing","", "Select [word]", "Select the named word."),

            // Advanced
            new("Advanced","", "Show grid", "Show the grid overlay for precise tapping."),
            new("Advanced","", "Show numbers continuously", "Keep numbers visible for touch targets."),
        };
    }

    protected sealed record VoiceAccessCommand(string Section, string Category, string Phrase, string Description);

    protected sealed record CommandGroupViewModel(string Section, IReadOnlyList<VoiceAccessCommand> Commands);
}
