using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Monkeyspeak.Lexical.Expressions
{
    public sealed class StringExpression : Expression<string>
    {
        public StringExpression()
        {
        }

        public StringExpression(SourcePosition pos, string value)
            : base(pos, value) { }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(GetValue<string>());
        }

        public override void Read(BinaryReader reader)
        {
            SetValue(reader.ReadString());
        }

        public override object Execute(Page page, Queue<IExpression> contents, bool processVariables = false)
        {
            try
            {
                var str = GetValue<string>();

                if (str[0] == '@')
                {
                    processVariables = false;
                    str = str.Substring(1);
                }

                if (processVariables)
                {
                    for (int i = page.Scope.Count - 1; i >= 0; i--)
                    {
                        var var = page.Scope[i];
                        object value = null;
                        // replaced string.replace with Regex because
                        //  %ListName would replace %ListName2 leaving the 2 at the end
                        //- Gerolkae
                        var pattern = var.Name + @"\b(\[[a-zA-Z_0-9]*\]+)?";
                        str = Regex.Replace(str, pattern, new MatchEvaluator(match =>
                        {
                            if (match.Success)
                            {
                                string val = match.Value;
                                if (val.IndexOf('[') != -1 && val.IndexOf(']') != -1)
                                {
                                    if (var is VariableTable)
                                    {
                                        value = (var as VariableTable)[val.Substring(val.IndexOf('[') + 1).TrimEnd(']')];
                                    }
                                }
                                else
                                    value = var.Value;
                            }
                            return value != null ? value.AsString() : "null";
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
    }
}