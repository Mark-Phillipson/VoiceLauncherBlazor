using System;
using System.Globalization;
using Humanizer;

namespace RazorClassLibrary.Services
{
    public static class TimeFormatter
    {
        public static string FormatTimestamp(DateTime createdAt, DateTime? now = null)
        {
            var nowUtc = (now ?? DateTime.UtcNow).ToUniversalTime();
            var createdUtc = createdAt.Kind == DateTimeKind.Utc ? createdAt : createdAt.ToUniversalTime();
            var age = nowUtc - createdUtc;
            if (age < TimeSpan.FromDays(1))
            {
                var human = age.Humanize(2);
                return human + " ago";
            }
            else
            {
                return createdUtc.ToLocalTime().ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture);
            }
        }
    }
}
