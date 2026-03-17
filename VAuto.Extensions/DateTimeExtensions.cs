using System;

namespace VAuto.Extensions
{
    /// <summary>
    /// Generic DateTime extension methods
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Format as relative time (e.g., "5m ago", "2h ago")
        /// </summary>
        public static string ToRelativeTime(this DateTime dt)
        {
            var now = DateTime.UtcNow;
            var diff = now - dt;

            if (diff.TotalSeconds < 60)
                return "just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";
            if (diff.TotalDays < 30)
                return $"{(int)(diff.TotalDays / 7)}w ago";
            if (diff.TotalDays < 365)
                return $"{(int)(diff.TotalDays / 30)}mo ago";

            return $"{(int)(diff.TotalDays / 365)}y ago";
        }

        /// <summary>
        /// Get Unix timestamp in seconds
        /// </summary>
        public static long ToUnixTime(this DateTime dt)
        {
            return new DateTimeOffset(dt).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Get Unix timestamp in milliseconds
        /// </summary>
        public static long ToUnixTimeMilliseconds(this DateTime dt)
        {
            return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Create DateTime from Unix timestamp in seconds
        /// </summary>
        public static DateTime FromUnixTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }

        /// <summary>
        /// Create DateTime from Unix timestamp in milliseconds
        /// </summary>
        public static DateTime FromUnixTimeMilliseconds(long unixTimeMs)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMs).DateTime;
        }

        /// <summary>
        /// Start of day
        /// </summary>
        public static DateTime StartOfDay(this DateTime dt)
        {
            return dt.Date;
        }

        /// <summary>
        /// End of day
        /// </summary>
        public static DateTime EndOfDay(this DateTime dt)
        {
            return dt.Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// Start of week (Monday)
        /// </summary>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Start of month
        /// </summary>
        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        /// <summary>
        /// End of month
        /// </summary>
        public static DateTime EndOfMonth(this DateTime dt)
        {
            return dt.StartOfMonth().AddMonths(1).AddTicks(-1);
        }

        /// <summary>
        /// Check if date is today
        /// </summary>
        public static bool IsToday(this DateTime dt)
        {
            return dt.Date == DateTime.Today;
        }

        /// <summary>
        /// Check if date is yesterday
        /// </summary>
        public static bool IsYesterday(this DateTime dt)
        {
            return dt.Date == DateTime.Today.AddDays(-1);
        }

        /// <summary>
        /// Check if date is tomorrow
        /// </summary>
        public static bool IsTomorrow(this DateTime dt)
        {
            return dt.Date == DateTime.Today.AddDays(1);
        }

        /// <summary>
        /// Get age from date
        /// </summary>
        public static int GetAge(this DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;
            return age;
        }

        /// <summary>
        /// Format as ISO 8601 string
        /// </summary>
        public static string ToIso8601(this DateTime dt)
        {
            return dt.ToUniversalTime().ToString("o");
        }

        /// <summary>
        /// Parse ISO 8601 string safely
        /// </summary>
        public static bool TryParseIso8601(string s, out DateTime result)
        {
            return DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out result);
        }
    }
}