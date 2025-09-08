namespace RazorClassLibrary.Models
{
    public class SelectionItem
    {
        // The value that will be returned/used by the caller (e.g. an ID or the string itself)
        public string Id { get; set; } = string.Empty;

        // The label shown to the user
        public string Label { get; set; } = string.Empty;

        // Optional bootstrap color class or custom class (e.g. "bg-success", "text-primary")
        public string? ColorClass { get; set; }
    }
}
