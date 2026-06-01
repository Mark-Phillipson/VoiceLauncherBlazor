using System;
using System.Globalization;

namespace RazorClassLibrary.Services
{
    public static class TimeFormatter
    {
        public static string FormatTimestamp(DateTime createdAt, DateTime? now = null)
        {
            var nowUtc = (now ?? DateTime.UtcNow).ToUniversalTime();
            var createdUtc = createdAt.Kind == DateTimeKind.Utc ? createdAt : createdAt.ToUniversalTime();
            var age = nowUtc - createdUtc;

            if (age < TimeSpan.FromMinutes(1))
            {
                return "just now";
            }
            else if (age < TimeSpan.FromHours(1))
            {
                var minutes = (int)Math.Floor(age.TotalMinutes);
                return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
            }
            else if (age < TimeSpan.FromDays(1))
            {
                var hours = (int)Math.Floor(age.TotalHours);
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }
            else
            {
                return createdUtc.ToLocalTime().ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture);
            }
        }
    }
}
