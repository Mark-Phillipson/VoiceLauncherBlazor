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
                                     || c.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
                                     || c.Phrase.Contains(term, StringComparison.OrdinalIgnoreCase)
                                     || c.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
            : AllCommands;

        // Group by Section + Category so Text Editing sub-groups appear as separate cards
        var grouped = query
            .GroupBy(c => string.IsNullOrWhiteSpace(c.Category) ? c.Section : $"{c.Section} – {c.Category}")
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
            // ── Navigation ──────────────────────────────────────────────────
            new("Navigation", "", "Go home", "Return to the Home screen."),
            new("Navigation", "", "Open [app]", "Open the named app."),
            new("Navigation", "", "Go back", "Navigate to the previous screen."),
            new("Navigation", "", "Show Control Center", "Open Control Center."),
            new("Navigation", "", "Show Notification Center", "Open Notification Center."),
            new("Navigation", "", "Lock screen", "Lock the device."),
            new("Navigation", "", "Show recent apps", "Show the app switcher."),
            new("Navigation", "", "Open App Library", "Open the App Library."),

            // ── System / Settings ────────────────────────────────────────────
            new("System", "", "Turn volume up", "Increase device volume."),
            new("System", "", "Turn volume down", "Decrease device volume."),
            new("System", "", "Mute", "Mute the device."),
            new("System", "", "Unmute", "Unmute the device."),
            new("System", "", "Take screenshot", "Capture a screenshot."),
            new("System", "", "Open Settings", "Open the Settings app."),
            new("System", "", "Turn on [Wi-Fi / Bluetooth / Dark Mode]", "Enable a named system setting."),
            new("System", "", "Turn off [Wi-Fi / Bluetooth / Dark Mode]", "Disable a named system setting."),
            new("System", "", "Stop listening", "Pause Voice Control (microphone sleeps)."),
            new("System", "", "Start listening", "Resume Voice Control."),

            // ── Overlays / Screen Labels ──────────────────────────────────────
            new("Overlays", "", "Show names", "Display item name labels on screen."),
            new("Overlays", "", "Show numbers", "Display number tags over each tappable item."),
            new("Overlays", "", "Show text numbers", "Show numbers beside text items."),
            new("Overlays", "", "Show grid", "Overlay a numbered grid for precise targeting."),
            new("Overlays", "", "Show grid continuously", "Keep the grid visible across commands."),
            new("Overlays", "", "Show numbers continuously", "Keep number labels visible across commands."),
            new("Overlays", "", "Hide names", "Remove name label overlay."),
            new("Overlays", "", "Hide numbers", "Remove number label overlay."),
            new("Overlays", "", "Hide grid", "Remove grid overlay."),

            // ── Interaction / Touch ───────────────────────────────────────────
            new("Interaction", "", "Tap [item name]", "Tap the named item."),
            new("Interaction", "", "Tap [number]", "Tap the item with that number label."),
            new("Interaction", "", "Long press [item]", "Long-press an item."),
            new("Interaction", "", "Double tap [item]", "Double-tap an item."),
            new("Interaction", "", "Drag [item] to [item]", "Drag one item onto another."),
            new("Interaction", "", "Repeat that", "Repeat the last action."),
            new("Interaction", "", "Repeat that [N] times", "Repeat the last action N times."),

            // ── Scrolling & Gestures ──────────────────────────────────────────
            new("Scrolling & Gestures", "", "Scroll up", "Scroll the current view up."),
            new("Scrolling & Gestures", "", "Scroll down", "Scroll the current view down."),
            new("Scrolling & Gestures", "", "Scroll left", "Scroll the current view left."),
            new("Scrolling & Gestures", "", "Scroll right", "Scroll the current view right."),
            new("Scrolling & Gestures", "", "Scroll to top", "Jump to the top of the page."),
            new("Scrolling & Gestures", "", "Scroll to bottom", "Jump to the bottom of the page."),
            new("Scrolling & Gestures", "", "Swipe up", "Perform an upward swipe gesture."),
            new("Scrolling & Gestures", "", "Swipe down", "Perform a downward swipe gesture."),
            new("Scrolling & Gestures", "", "Swipe left", "Perform a leftward swipe gesture."),
            new("Scrolling & Gestures", "", "Swipe right", "Perform a rightward swipe gesture."),
            new("Scrolling & Gestures", "", "Zoom in", "Pinch-zoom in on content."),
            new("Scrolling & Gestures", "", "Zoom out", "Pinch-zoom out on content."),
            new("Scrolling & Gestures", "", "Pinch", "Perform a pinch gesture."),
            new("Scrolling & Gestures", "", "Rotate left / right", "Rotate content left or right."),

            // ── Text Input ────────────────────────────────────────────────────
            new("Text Input", "", "Tap [text field]", "Focus the named text field."),
            new("Text Input", "", "[word or phrase]", "Dictate text into the current field."),
            new("Text Input", "", "New line", "Insert a line break."),
            new("Text Input", "", "New paragraph", "Insert a paragraph break."),
            new("Text Input", "", "Dictation mode", "Dictate word by word (default)."),
            new("Text Input", "", "Spelling mode", "Enter text character by character."),
            new("Text Input", "", "Command mode", "Accept commands only; ignore dictated words."),
            new("Text Input", "", "Format email", "Format nearby text as an email address."),
            new("Text Input", "", "Insert [phrase] before [word]", "Insert text before the named word."),
            new("Text Input", "", "Insert [phrase] after [word]", "Insert text after the named word."),

            // ── Text Editing / Select ─────────────────────────────────────────
            new("Text Editing", "Select", "Select [word]", "Select the named word."),
            new("Text Editing", "Select", "Select all", "Select all text."),
            new("Text Editing", "Select", "Select [word] through [word]", "Select a range of text."),
            new("Text Editing", "Select", "Select to beginning", "Select from cursor to start."),
            new("Text Editing", "Select", "Select to end", "Select from cursor to end."),
            new("Text Editing", "Select", "Select next [N] words", "Select the next N words."),
            new("Text Editing", "Select", "Select previous [N] words", "Select the previous N words."),
            new("Text Editing", "Select", "Unselect all", "Deselect the current selection."),

            // ── Text Editing / Delete ─────────────────────────────────────────
            new("Text Editing", "Delete", "Delete that", "Delete the current selection."),
            new("Text Editing", "Delete", "Delete last word", "Delete the word before the cursor."),
            new("Text Editing", "Delete", "Delete [word/phrase]", "Delete specific text."),
            new("Text Editing", "Delete", "Delete all", "Delete all text in the field."),
            new("Text Editing", "Delete", "Delete to beginning", "Delete everything before the cursor."),
            new("Text Editing", "Delete", "Delete to end", "Delete everything after the cursor."),
            new("Text Editing", "Delete", "Delete next [N] words", "Delete the next N words."),
            new("Text Editing", "Delete", "Delete previous [N] sentences", "Delete the previous N sentences."),

            // ── Text Editing / Format ─────────────────────────────────────────
            new("Text Editing", "Format", "Capitalize [word]", "Capitalize a word."),
            new("Text Editing", "Format", "Uppercase [word]", "Convert a word to uppercase."),
            new("Text Editing", "Format", "Lowercase [word]", "Convert a word to lowercase."),
            new("Text Editing", "Format", "Replace [word] with [word]", "Replace one word or phrase with another."),
            new("Text Editing", "Format", "Bold [word]", "Apply bold formatting."),
            new("Text Editing", "Format", "Italic [word]", "Apply italic formatting."),
            new("Text Editing", "Format", "Underline [word]", "Apply underline formatting."),

            // ── Text Editing / Move / Actions ─────────────────────────────────
            new("Text Editing", "Move & Actions", "Move to beginning", "Move cursor to the start."),
            new("Text Editing", "Move & Actions", "Move to end", "Move cursor to the end."),
            new("Text Editing", "Move & Actions", "Move after [word]", "Move cursor after the named word."),
            new("Text Editing", "Move & Actions", "Move before [word]", "Move cursor before the named word."),
            new("Text Editing", "Move & Actions", "Cut", "Cut the selected text."),
            new("Text Editing", "Move & Actions", "Copy", "Copy the selected text."),
            new("Text Editing", "Move & Actions", "Paste", "Paste from clipboard."),
            new("Text Editing", "Move & Actions", "Undo", "Undo the last action."),
            new("Text Editing", "Move & Actions", "Redo", "Redo the last undone action."),

            // ── Help ──────────────────────────────────────────────────────────
            new("Help", "", "Show commands", "Display available commands for the current context."),
            new("Help", "", "Show me what to say", "Show command hints for the current screen."),
        };
    }

    protected sealed record VoiceAccessCommand(string Section, string Category, string Phrase, string Description);

    protected sealed record CommandGroupViewModel(string Section, IReadOnlyList<VoiceAccessCommand> Commands);
}
