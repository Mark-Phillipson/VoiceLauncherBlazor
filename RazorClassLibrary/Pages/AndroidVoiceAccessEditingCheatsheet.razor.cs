using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace RazorClassLibrary.Pages;

public partial class AndroidVoiceAccessEditingCheatsheet : ComponentBase
{
    private static readonly IReadOnlyList<VoiceAccessCommand> AllCommands = BuildCommands();
    private readonly List<CommandGroupViewModel> filteredGroups = new();

    private string searchTerm = string.Empty;
    private ElementReference searchInput;

    protected IReadOnlyList<CommandGroupViewModel> FilteredGroups => filteredGroups;

    protected int FilteredCount { get; private set; }

    protected string StatusText => string.IsNullOrWhiteSpace(SearchTerm)
        ? $"Showing all {FilteredCount} commands across {FilteredGroups.Count} categories."
        : $"Showing {FilteredCount} matching commands for '{SearchTerm}' across {FilteredGroups.Count} categories.";

    protected string SearchTerm
    {
        get => searchTerm;
        set
        {
            searchTerm = value;
            ApplyFilter();
        }
    }

    protected override void OnInitialized()
    {
        ApplyFilter();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await searchInput.FocusAsync();
        }
    }

    protected void ClearSearch()
    {
        SearchTerm = string.Empty;
    }

    private void ApplyFilter()
    {
        var term = SearchTerm.Trim();
        var hasTerm = !string.IsNullOrWhiteSpace(term);

        var query = hasTerm
            ? AllCommands.Where(command =>
                command.Section.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                command.Category.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                command.Phrase.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                command.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
            : AllCommands;

        var grouped = query
            .GroupBy(command => new { command.Section, command.Category })
            .OrderBy(group => SectionSortIndex(group.Key.Section))
            .ThenBy(group => CategorySortIndex(group.Key.Section, group.Key.Category))
            .ThenBy(group => group.Key.Category)
            .Select(group => new CommandGroupViewModel(
                group.Key.Section,
                group.Key.Category,
                ToDomId($"{group.Key.Section}-{group.Key.Category}"),
                group.OrderBy(command => command.Phrase).ToList()))
            .ToList();

        filteredGroups.Clear();
        filteredGroups.AddRange(grouped);
        FilteredCount = query.Count();
    }

    private static int SectionSortIndex(string section) => section switch
    {
        "General" => 0,
        "Gestures" => 1,
        "Magnifying glass" => 2,
        "Text editing" => 3,
        _ => 99
    };

    private static int CategorySortIndex(string section, string category)
    {
        if (section == "General")
        {
            return category switch
            {
                "Navigation" => 0,
                "Ask for help" => 1,
                "Adjust settings" => 2,
                "Assistant" => 3,
                _ => 99
            };
        }

        if (section == "Gestures")
        {
            return category switch
            {
                "Touch" => 0,
                "Swipe" => 1,
                _ => 99
            };
        }

        if (section == "Text editing")
        {
            return category switch
            {
                "Typing" => 0,
                "Replace" => 1,
                "Delete" => 2,
                "Move" => 3,
                "Select" => 4,
                "Actions" => 5,
                "Keyboard" => 6,
                _ => 99
            };
        }

        return 99;
    }

    private static string ToDomId(string value)
    {
        var normalized = value.ToLowerInvariant();
        var chars = normalized
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        return new string(chars);
    }

    private static List<VoiceAccessCommand> BuildCommands()
    {
        return new List<VoiceAccessCommand>
        {
            // General / Navigation
            new("General", "Navigation", "Open <app>", "Opens the given app."),
            new("General", "Navigation", "Go back", "Presses the back button."),
            new("General", "Navigation", "Go home", "Presses the home button."),
            new("General", "Navigation", "Show notifications", "Shows notifications."),
            new("General", "Navigation", "Show Quick Settings", "Displays quick settings."),
            new("General", "Navigation", "Show recent apps", "Shows recent apps."),

            // General / Ask for help
            new("General", "Ask for help", "What can I say?", "Shows commands you can say."),
            new("General", "Ask for help", "Show all commands", "Shows all available commands."),
            new("General", "Ask for help", "Open tutorial", "Opens the Voice Access tutorial."),
            new("General", "Ask for help", "Hide numbers", "Hides numbers on the screen."),
            new("General", "Ask for help", "Show numbers", "Shows numbers on the screen."),
            new("General", "Ask for help", "What is <number>?", "Explains the element type for a number."),
            new("General", "Ask for help", "Stop Voice Access", "Stops Voice Access."),
            new("General", "Ask for help", "Send feedback", "Opens feedback for Voice Access."),

            // General / Adjust settings
            new("General", "Adjust settings", "Turn <on/off> <setting>", "Turns a setting on or off, for example Wi-Fi."),
            new("General", "Adjust settings", "Turn <up/down> volume", "Turns overall volume up or down."),
            new("General", "Adjust settings", "Turn <media/alarm/phone> volume <up/down>", "Adjusts media, alarm, or phone volume."),
            new("General", "Adjust settings", "Mute", "Turns off sound."),
            new("General", "Adjust settings", "Silence", "Mutes sound."),
            new("General", "Adjust settings", "Unmute", "Turns sound back on."),
            new("General", "Adjust settings", "<Mute/unmute> <media/alarm/phone> volume", "Mutes or unmutes a specific volume channel."),
            new("General", "Adjust settings", "Turn device off", "Turns off the device."),

            // General / Assistant
            new("General", "Assistant", "Set timer for <time>", "Starts a timer for the given time."),
            new("General", "Assistant", "Turn on flashlight", "Turns on the flashlight."),
            new("General", "Assistant", "When was the Empire State Building built?", "Asks Assistant a factual question."),
            new("General", "Assistant", "Who created Google?", "Asks Assistant a factual question."),

            // Gestures / Touch
            new("Gestures", "Touch", "Touch <element>", "Touches the given element."),
            new("Gestures", "Touch", "<element>", "Touches the given element."),
            new("Gestures", "Touch", "Long press <element>", "Long-presses the given element."),
            new("Gestures", "Touch", "Switch <on/off> <setting>", "Switches a setting on or off."),
            new("Gestures", "Touch", "Expand <element>", "Expands the given element."),
            new("Gestures", "Touch", "Collapse <element>", "Collapses the given element."),

            // Gestures / Swipe
            new("Gestures", "Swipe", "Scroll <left/right/up/down>", "Scrolls in the given direction."),
            new("Gestures", "Swipe", "Scroll to <top/bottom>", "Scrolls to the top or bottom."),
            new("Gestures", "Swipe", "Swipe <forwards/backwards>", "Swipes forward or backward."),

            // Magnifying glass
            new("Magnifying glass", "Zoom and pan", "Start magnification", "Starts magnification mode."),
            new("Magnifying glass", "Zoom and pan", "Start zooming", "Starts zoom mode."),
            new("Magnifying glass", "Zoom and pan", "Magnify", "Increases magnification."),
            new("Magnifying glass", "Zoom and pan", "Enhance", "Improves visibility with magnification."),
            new("Magnifying glass", "Zoom and pan", "Zoom in", "Zooms in."),
            new("Magnifying glass", "Zoom and pan", "Zoom out", "Zooms out."),
            new("Magnifying glass", "Zoom and pan", "Pan <left/right/up/down>", "Pans in a direction."),
            new("Magnifying glass", "Zoom and pan", "Move <left/right/up/down>", "Moves in a direction while zoomed."),
            new("Magnifying glass", "Zoom and pan", "Go <left/right/up/down>", "Moves in a direction while zoomed."),
            new("Magnifying glass", "Zoom and pan", "Stop magnification", "Stops magnification."),
            new("Magnifying glass", "Zoom and pan", "Stop zooming", "Stops zooming."),
            new("Magnifying glass", "Zoom and pan", "Cancel zoom", "Cancels zoom."),

            // Text editing / Typing
            new("Text editing", "Typing", "Type <word/phrase>", "Inserts the given text."),
            new("Text editing", "Typing", "<word/phrase>", "Inserts the given text."),
            new("Text editing", "Typing", "Undo", "Undoes the previous action."),
            new("Text editing", "Typing", "Redo", "Redoes the previous action."),
            new("Text editing", "Typing", "Insert <word/phrase> <before/after/between> <word/phrase>", "Places text before, after, or between text."),
            new("Text editing", "Typing", "Format email", "Formats nearby text as an email address."),

            // Text editing / Replace
            new("Text editing", "Replace", "Replace <word/phrase> with <word/phrase>", "Replaces given text with desired text."),
            new("Text editing", "Replace", "Replace everything between <word/phrase> and <word/phrase> with <word/phrase>", "Replaces all text between two phrases."),
            new("Text editing", "Replace", "Capitalize <word/phrase>", "Capitalizes text."),
            new("Text editing", "Replace", "Uppercase <word/phrase>", "Converts text to uppercase."),
            new("Text editing", "Replace", "Lowercase <word/phrase>", "Converts text to lowercase."),

            // Text editing / Delete
            new("Text editing", "Delete", "Delete", "Deletes text."),
            new("Text editing", "Delete", "Delete all", "Deletes everything."),
            new("Text editing", "Delete", "Delete <word/phrase>", "Deletes specific text."),
            new("Text editing", "Delete", "Delete to the beginning", "Deletes everything to the beginning."),
            new("Text editing", "Delete", "Delete to the end", "Deletes everything to the end."),
            new("Text editing", "Delete", "Delete selected text", "Deletes selected text."),
            new("Text editing", "Delete", "Delete from <word/phrase> to <word/phrase>", "Deletes text between two phrases."),
            new("Text editing", "Delete", "Delete <number> <characters/words/sentences/lines/paragraphs/pages>", "Deletes a quantity of a text unit."),
            new("Text editing", "Delete", "Delete the <next/previous> <number> <characters/words/sentences/lines/paragraphs/pages>", "Deletes next or previous quantity of a text unit."),

            // Text editing / Move
            new("Text editing", "Move", "Go to the beginning", "Moves cursor to the beginning."),
            new("Text editing", "Move", "Go to the end", "Moves cursor to the end."),
            new("Text editing", "Move", "Move <before/after> <word/phrase>", "Moves cursor before or after text."),
            new("Text editing", "Move", "Move between <word/phrase> and <word/phrase>", "Moves cursor between two phrases."),
            new("Text editing", "Move", "<right/left> <number> <characters/words/sentences/lines/paragraphs/pages>", "Moves cursor by quantity and text unit."),

            // Text editing / Select
            new("Text editing", "Select", "Select all text", "Selects all text."),
            new("Text editing", "Select", "Unselect all text", "Deselects all text."),
            new("Text editing", "Select", "Select to the beginning", "Selects to beginning."),
            new("Text editing", "Select", "Select to the end", "Selects to end."),
            new("Text editing", "Select", "Select <word/phrase>", "Selects specific text."),
            new("Text editing", "Select", "Select from <word/phrase> to <word/phrase>", "Selects text between two phrases."),
            new("Text editing", "Select", "Select <number> <characters/words/sentences/lines/paragraphs/pages>", "Selects quantity of text unit."),
            new("Text editing", "Select", "Select the <next/previous> <number> <characters/words/sentences/lines/paragraphs/pages>", "Selects next or previous quantity of text unit."),

            // Text editing / Actions
            new("Text editing", "Actions", "Cut", "Cuts selected text."),
            new("Text editing", "Actions", "Copy", "Copies selected text."),
            new("Text editing", "Actions", "Paste", "Pastes clipboard content."),

            // Text editing / Keyboard
            new("Text editing", "Keyboard", "Show keyboard", "Shows the keyboard."),
            new("Text editing", "Keyboard", "Hide keyboard", "Hides the keyboard.")
        };
    }

    protected sealed record VoiceAccessCommand(string Section, string Category, string Phrase, string Description);

    protected sealed record CommandGroupViewModel(string Section, string Category, string SectionId, IReadOnlyList<VoiceAccessCommand> Commands);
}
