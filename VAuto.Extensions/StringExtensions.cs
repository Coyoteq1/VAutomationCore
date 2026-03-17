namespace VAuto.Extensions
{
    /// <summary>
    /// Generic string extension methods
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Repeat string n times
        /// </summary>
        public static string Repeat(this string str, int count)
        {
            if (string.IsNullOrEmpty(str) || count <= 0)
                return string.Empty;
            return string.Concat(System.Linq.Enumerable.Repeat(str, count));
        }

        /// <summary>
        /// Truncate string to max length
        /// </summary>
        public static string Truncate(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Length <= maxLength ? str : str[..maxLength];
        }

        /// <summary>
        /// Convert to title case
        /// </summary>
        public static string ToTitleCase(this string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Check if string contains substring (case insensitive)
        /// </summary>
        public static bool ContainsIgnoreCase(this string source, string value)
        {
            return source?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Safely format string with arguments
        /// </summary>
        public static string FormatSafe(this string format, params object[] args)
        {
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return format;
            }
        }

        /// <summary>
        /// Remove whitespace from string
        /// </summary>
        public static string RemoveWhitespace(this string str)
        {
            return string.Concat(str.Where(c => !char.IsWhiteSpace(c)));
        }

        /// <summary>
        /// Reverse the string
        /// </summary>
        public static string Reverse(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            char[] chars = str.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Check if string is null or empty
        /// </summary>
        public static bool IsNullOrEmpty(this string? str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Check if string is null, empty, or whitespace
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string? str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Get substring before first occurrence of delimiter
        /// </summary>
        public static string SubstringBefore(this string str, string delimiter)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(delimiter))
                return str;
            
            int index = str.IndexOf(delimiter, StringComparison.Ordinal);
            return index < 0 ? str : str[..index];
        }

        /// <summary>
        /// Get substring after first occurrence of delimiter
        /// </summary>
        public static string SubstringAfter(this string str, string delimiter)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(delimiter))
                return str;
            
            int index = str.IndexOf(delimiter, StringComparison.Ordinal);
            return index < 0 ? string.Empty : str[(index + delimiter.Length)..];
        }
    }
}