using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Monkeyspeak.Lexical.Expressions
{
    public sealed class StringExpression : Expression<string>
    {
        private bool humanReadableNumbers = true;
        private List<char> specialPrefixes = new List<char>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StringExpression"/> class.
        /// </summary>
        public StringExpression()
        {
            specialPrefixes.Add('\\');
            specialPrefixes.Add('@');
            specialPrefixes.Add('!');
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringExpression"/> class.
        /// </summary>
        /// <param name="pos">  The position.</param>
        /// <param name="value">The value.</param>
        public StringExpression(SourcePosition pos, string value)
            : base(pos, value)
        {
            specialPrefixes.Add('\\');
            specialPrefixes.Add('@');
            specialPrefixes.Add('!');
        }

        /// <summary>
        /// Writes the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void Write(BinaryWriter writer)
        {
            writer.Write(GetValue<string>());
        }

        /// <summary>
        /// Reads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public override void Read(BinaryReader reader)
        {
            SetValue(reader.ReadString());
        }

        /// <summary>
        /// Executes the specified page.
        /// </summary>
        /// <param name="page">            The page.</param>
        /// <param name="contents">        The contents.</param>
        /// <param name="processVariables">if set to <c>true</c> [process variables].</param>
        /// <returns></returns>
        /// <exception cref="TriggerReaderException"></exception>
        public override object Execute(Page page, Queue<IExpression> contents, bool processVariables = false)
        {
            try
            {
                var str = GetValue<string>();
                bool negatePrefix = false;
                while (specialPrefixes.Contains(str[0]))
                {
                    if (str[0] == '\\')
                    {
                        if (negatePrefix)
                        {
                            negatePrefix = false;
                        }
                        else
                        {
                            negatePrefix = true;
                        }
                    }
                    else if (str[0] == '@')
                    {
                        if (!negatePrefix) processVariables = false;
                        str = str.Slice(negatePrefix ? 2 : 1, -1);
                        negatePrefix = false;
                    }
                    else if (str[0] == '!')
                    {
                        if (!negatePrefix) humanReadableNumbers = false;
                        str = str.Slice(negatePrefix ? 2 : 1, -1);
                        negatePrefix = false;
                    }
                }

                if (processVariables)
                {
                    for (int i = page.Scope.Count - 1; i >= 0; i--)
                    {
                        var var = page.Scope[i];
                        object value = null;
                        var pattern = var.Name + @"(?:(\b(\[[a-zA-Z_0-9\$\@]*\]+))|(\b(\.[a-zA-Z_0-9]+)))?";
                        str = Regex.Replace(str, pattern, new MatchEvaluator(match =>
                        {
                            if (match.Success)
                            {
                                string val = match.Value;
                                if (val.IndexOf('[') != -1 && val.IndexOf(']') != -1)
                                {
                                    if (var is VariableTable)
                                    {
                                        value = (var as VariableTable)[val.RightOf('[').LeftOf(']')];
                                    }
                                }
                                else if (val.IndexOf('.') != -1)
                                {
                                    if (var is ObjectVariable)
                                    {
                                        (var as ObjectVariable).DesiredProperty = val.RightOf('.');
                                        value = var.Value;
                                    }
                                }
                                else
                                    value = var.Value;
                            }

                            if (value != null)
                            {
                                string result = value.AsString();
                                if (humanReadableNumbers && value is double)
                                {
                                    NumberFormatInfo nfo = new NumberFormatInfo
                                    {
                                        CurrencyGroupSeparator = ",",
                                        CurrencyGroupSizes = new int[] { 3, 2 },
                                        CurrencySymbol = ""
                                    };
                                    return String.Format(CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"), "{0:n0}", value);
                                }
                                else return result;
                            }
                            return "null";
                        }), RegexOptions.CultureInvariant);
                    }
                }
                return str;
            }
            catch (Exception ex)
            {
                Logger.Error<TriggerReader>(ex);
                throw new TriggerReaderException($"No value found at {Position}");
            }
        }

        public override string ToString()
        {
            return GetValue<string>();
        }
    }
}