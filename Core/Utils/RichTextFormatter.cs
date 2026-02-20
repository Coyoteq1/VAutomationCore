using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VAutomationCore.Utils
{
    /// <summary>
    /// Rich text formatting utility with color constants and preset message formatters
    /// for Unity-compatible rich text markup.
    /// </summary>
    public static class RichTextFormatter
    {
        // ============== Color Constants ==============
        
        /// <summary>Red color (#FF5555)</summary>
        public const string Red = "#FF5555";
        /// <summary>Green color (#55FF55)</summary>
        public const string Green = "#55FF55";
        /// <summary>Blue color (#5555FF)</summary>
        public const string Blue = "#5555FF";
        /// <summary>Yellow color (#FFFF55)</summary>
        public const string Yellow = "#FFFF55";
        /// <summary>Cyan color (#55FFFF)</summary>
        public const string Cyan = "#55FFFF";
        /// <summary>Magenta color (#FF55FF)</summary>
        public const string Magenta = "#FF55FF";
        /// <summary>White color (#FFFFFF)</summary>
        public const string White = "#FFFFFF";
        /// <summary>Gray color (#AAAAAA)</summary>
        public const string Gray = "#AAAAAA";
        /// <summary>Orange color (#FFAA00)</summary>
        public const string Orange = "#FFAA00";
        /// <summary>Purple color (#AA55FF)</summary>
        public const string Purple = "#AA55FF";
        /// <summary>Pink color (#FF77AA)</summary>
        public const string Pink = "#FF77AA";
        /// <summary>Lime color (#AAFF55)</summary>
        public const string Lime = "#AAFF55";
        
        // ============== Basic Formatting ==============
        
        /// <summary>
        /// Wrap text in bold tags.
        /// </summary>
        public static string Bold(string text) => $"<b>{text}</b>";
        
        /// <summary>
        /// Wrap text in italic tags.
        /// </summary>
        public static string Italic(string text) => $"<i>{text}</i>";
        
        /// <summary>
        /// Wrap text in underline tags.
        /// </summary>
        public static string Underline(string text) => $"<u>{text}</u>";
        
        /// <summary>
        /// Wrap text in color tags.
        /// </summary>
        public static string WithColor(string text, string color) => $"<color={color}>{text}</color>";
        
        // ============== Preset Formatters ==============
        
        /// <summary>
        /// Format as error message (red).
        /// </summary>
        public static string Error(string message) => $"<color={Red}>✖ {message}</color>";
        
        /// <summary>
        /// Format as warning message (yellow).
        /// </summary>
        public static string Warning(string message) => $"<color={Yellow}>⚠ {message}</color>";
        
        /// <summary>
        /// Format as success message (green).
        /// </summary>
        public static string Success(string message) => $"<color={Green}>✓ {message}</color>";
        
        /// <summary>
        /// Format as info message (cyan).
        /// </summary>
        public static string Info(string message) => $"<color={Cyan}>ℹ {message}</color>";
        
        /// <summary>
        /// Format as system message (white).
        /// </summary>
        public static string System(string message) => $"<color={White}>◆ {message}</color>";
        
        // ============== Markdown Syntax ==============
        
        private static readonly Regex BoldRegex = new Regex(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
        private static readonly Regex ItalicRegex = new Regex(@"\*(.+?)\*", RegexOptions.Compiled);
        private static readonly Regex UnderlineRegex = new Regex(@"__(.+?)__", RegexOptions.Compiled);
        private static readonly Regex HighlightRegex = new Regex(@"~(.+?)~", RegexOptions.Compiled);
        
        /// <summary>
        /// Process markdown-style formatting: **bold**, *italic*, __underline__, ~highlight~.
        /// </summary>
        public static string Format(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            text = BoldRegex.Replace(text, "<b>$1</b>");
            text = ItalicRegex.Replace(text, "<i>$1</i>");
            text = UnderlineRegex.Replace(text, "<u>$1</u>");
            text = HighlightRegex.Replace(text, "<color=#FFFF55>$1</color>");
            
            return text;
        }
        
        // ============== Background Highlighting ==============
        
        /// <summary>
        /// Wrap text with background highlight (^text^).
        /// </summary>
        public static string WithBackground(string text, string bgColor)
        {
            return $"<mark={bgColor}>{text}</mark>";
        }
        
        // ============== Player Event Messages ==============
        
        /// <summary>
        /// Format player join message.
        /// </summary>
        public static string PlayerJoin(string playerName) => 
            $"<color={Green}>► {playerName} joined the game</color>";
        
        /// <summary>
        /// Format player leave message.
        /// </summary>
        public static string PlayerLeave(string playerName) => 
            $"<color={Gray}>◄ {playerName} left the game</color>";
        
        /// <summary>
        /// Format player death message.
        /// </summary>
        public static string PlayerDeath(string playerName, string killerName) => 
            $"<color={Red}>☠ {playerName} was slain by {killerName}</color>";
        
        // ============== List Formatting ==============
        
        /// <summary>
        /// Format bullet list item.
        /// </summary>
        public static string Bullet(string text) => $"  • {text}";
        
        /// <summary>
        /// Format numbered list item.
        /// </summary>
        public static string Numbered(int number, string text) => $"  {number}. {text}";
        
        /// <summary>
        /// Format as header (bold + color).
        /// </summary>
        public static string Header(string text, string color = White) => 
            $"<b><color={color}>{text}</color></b>";
        
        // ============== Cached Formatting ==============
        
        private static readonly Dictionary<string, string> _formatCache = new();
        
        /// <summary>
        /// Get or create formatted string with caching (for repeated use).
        /// </summary>
        public static string GetCached(string key, string format, params object[] args)
        {
            if (!_formatCache.TryGetValue(key, out var cached))
            {
                cached = string.Format(format, args);
                _formatCache[key] = cached;
            }
            return cached;
        }
        
        /// <summary>
        /// Clear format cache (call when language changes).
        /// </summary>
        public static void ClearCache() => _formatCache.Clear();
    }
}
