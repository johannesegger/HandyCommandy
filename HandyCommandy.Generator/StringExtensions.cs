using System.Collections.Generic;

namespace HandyCommandy.Generator
{
    public static class StringExtensions
    {
        public static string FirstToUpper(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }

        public static string FirstToLower(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }

        public static string Join(this IEnumerable<string> values, string separator)
        {
            return string.Join(separator, values);
        }
    }
}
