using System;

namespace RazorClassLibrary.Models
{
    public class ClipboardEntry
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
    }

    public enum ClipboardHistorySortOrder
    {
        NewestFirst,
        OldestFirst
    }
}
