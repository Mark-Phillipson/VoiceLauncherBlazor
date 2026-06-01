using System;
using System.Collections.Generic;
using System.Linq;

namespace RazorClassLibrary.Services
{
    public static class AccessKeyMapper
    {
        // Conservative avoid list selected by user (uppercase chars)
        private static readonly HashSet<char> AvoidLetters = new HashSet<char>("CVSRFTK".ToCharArray());

        private static readonly char[] LetterPool = Enumerable.Range('a', 26).Select(i => (char)i).Where(c => !AvoidLetters.Contains(char.ToUpperInvariant(c))).ToArray();

        public static string? GetKeyForIndex(int index)
        {
            if (index < 0) return null;
            if (index <= 9)
            {
                return index.ToString();
            }

            var letterIndex = (index - 10) % LetterPool.Length;
            return LetterPool[letterIndex].ToString();
        }

        public static string GetAriaKeyShortcuts(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return $"Alt+{key}";
        }
    }
}
