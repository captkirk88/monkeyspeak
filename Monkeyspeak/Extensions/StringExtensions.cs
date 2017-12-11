using System.Text;

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
                    case '"':
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

                case '"':
                    return "\\\"";

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
    }
}