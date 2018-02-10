using System;
using System.Text;
using System.Linq;

namespace Monkeyspeak.Extensions
{
    public static class StringExtensions
    {
        public static string EscapeForCSharp(this string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
                switch (c)
                {
                    case '\'':
                    case '\\':
                        sb.Append(c.EscapeForCSharp());
                        break;

                    default:
                        if (char.IsControl(c))
                            sb.Append(c.EscapeForCSharp());
                        else
                            sb.Append(c);
                        break;
                }
            return sb.ToString();
        }

        public static string EscapeForCSharp(this char chr)
        {
            switch (chr)
            {
                case '\'':
                    return @"\'";

                case '\\':
                    return @"\\";

                case '\0':
                    return @"\0";

                case '\a':
                    return @"\a";

                case '\b':
                    return @"\b";

                case '\f':
                    return @"\f";

                case '\n':
                    return @"\n";

                case '\r':
                    return @"\r";

                case '\t':
                    return @"\t";

                case '\v':
                    return @"\v";

                default:
                    if (char.IsControl(chr) || char.IsHighSurrogate(chr) || char.IsLowSurrogate(chr))
                        return @"\u" + ((int)chr).ToString("X4");
                    else
                        return new string(chr, 1);
            }
        }

        public static bool IsNullOrBlank(this string str)
        {
            return string.IsNullOrEmpty(str) || (str.Length == 1 && str[0] == ' ');
        }

        public static string LeftOf(this string str, char c)
        {
            int index = str.IndexOf(c);
            return (index > 0 ? str.Slice(0, index + 1) : "");
        }

        public static string LeftOf(this string str, string c)
        {
            int index = str.IndexOf(c);
            return (index > 0 ? str.Substring(0, index + 1) : "");
        }

        public static string RightMostLeftOf(this string str, char c)
        {
            int index = str.LastIndexOf(c);
            return (index > 0 ? str.Substring(0, index + 1) : "");
        }

        public static string RightMostLeftOf(this string str, string c)
        {
            int index = str.LastIndexOf(c);
            return (index > 0 ? str.Substring(0, index + 1) : "");
        }

        public static string RightOf(this string str, char c)
        {
            int index = str.IndexOf(c);
            return (index > 0 ? str.Substring(index + 1, str.Length - (index + 1)) : "");
        }

        public static string RightOf(this string str, string c)
        {
            int index = str.IndexOf(c);
            return (index > 0 ? str.Substring(index + c.Length, str.Length - (index + c.Length)) : "");
        }

        public static string RightMostRightOf(this string str, char c)
        {
            int index = str.LastIndexOf(c);
            return (index > 0 ? str.Substring(index + 1, str.Length - (index + 1)) : "");
        }

        public static string RightMostRightOf(this string str, string c)
        {
            int index = str.LastIndexOf(c);
            return (index > 0 ? str.Substring(index + c.Length, str.Length - (index + c.Length)) : "");
        }

        public static string Slice(this string str, int start, int end)
        {
            if (end < 0)
            {
                end = str.Length + end;
            }
            int len = end - start;               // Calculate length
            return str.Substring(start, len); // Return Substring of length
        }

        /// <summary>
        /// Calculates the Levenshtein distance between two strings, smaller distance the better of a match.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int LevenshteinDistance(this string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;
            for (int j = 0; j <= m; d[0, j] = j++)
                ;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        public static string ToBase64(this string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        public static string FromBase64(this string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }
    }
}