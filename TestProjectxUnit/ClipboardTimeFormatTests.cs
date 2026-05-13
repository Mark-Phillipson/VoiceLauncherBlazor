using System;
using RazorClassLibrary.Services;
using Xunit;

namespace TestProjectxUnit
{
    public class ClipboardTimeFormatTests
    {
        [Fact]
        public void RecentTimestamp_IsRelative()
        {
            var now = new DateTime(2026, 5, 13, 12, 0, 0, DateTimeKind.Utc);
            var created = now.AddMinutes(-5);
            var formatted = TimeFormatter.FormatTimestamp(created, now);
            Assert.Equal("5 minutes ago", formatted);
        }

        [Fact]
        public void OlderTimestamp_IsAbsolute()
        {
            var now = new DateTime(2026, 5, 13, 12, 0, 0, DateTimeKind.Local);
            var created = new DateTime(2026, 4, 11, 10, 34, 0, DateTimeKind.Local);
            var formatted = TimeFormatter.FormatTimestamp(created, now);
            Assert.Equal("Apr 11, 2026 10:34 AM", formatted);
        }
    }
}
