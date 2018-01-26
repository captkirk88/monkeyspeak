using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.HelperClasses
{
    internal class MonkeyspeakIndentationStrategy : IIndentationStrategy
    {
        private readonly Page page;
        private Lexer lexer;
        private Parser parser;

        public MonkeyspeakIndentationStrategy()
        {
            parser = new Parser(MonkeyspeakRunner.Engine);
        }

        public void IndentLine(TextDocument document, DocumentLine line)
        {
            using (var memory = new MemoryStream(Encoding.Default.GetBytes(document.GetText(line))))
            {
                try
                {
                    lexer = new Lexer(page.Engine, new SStreamReader(memory));
                    var trigger = parser.Parse(lexer).FirstOrDefault();
                    if (trigger != default(Trigger))
                    {
                        int indentCount = 0;
                        switch (trigger.Category)
                        {
                            case TriggerCategory.Cause:
                                indentCount = 1;
                                break;

                            case TriggerCategory.Condition:
                                indentCount = 2;
                                break;

                            case TriggerCategory.Effect:
                                indentCount = 3;
                                break;

                            case TriggerCategory.Flow:
                                indentCount = 2;
                                break;
                        }
                        var sb = new StringBuilder();
                        for (int i = 0; i <= indentCount - 1; i++) sb.Append('\t');
                        var indentationSegment = TextUtilities.GetLeadingWhitespace(document, line);
                        document.Replace(indentationSegment.Offset, indentationSegment.Length, sb.ToString(),
                            OffsetChangeMappingType.RemoveAndInsert);
                    }
                }
                catch { }
            }
        }

        public void IndentLines(TextDocument document, int beginLine, int endLine)
        {
            for (int i = beginLine; i < endLine; i++)
            {
                var line = document.GetLineByNumber(i);
                using (var memory = new MemoryStream(Encoding.Default.GetBytes(document.GetText(line))))
                {
                    try
                    {
                        lexer = new Lexer(page.Engine, new SStreamReader(memory));
                        foreach (var trigger in parser.Parse(lexer))
                        {
                            Logger.Debug(trigger);
                            if (trigger != default(Trigger))
                            {
                                int indentCount = 0;
                                switch (trigger.Category)
                                {
                                    case TriggerCategory.Cause:
                                        indentCount = 1;
                                        break;

                                    case TriggerCategory.Condition:
                                        indentCount = 2;
                                        break;

                                    case TriggerCategory.Effect:
                                        indentCount = 3;
                                        break;

                                    case TriggerCategory.Flow:
                                        indentCount = 4;
                                        break;
                                }
                                var sb = new StringBuilder();
                                for (int t = 0; t <= indentCount - 1; t++) sb.Append('\t');
                                var indentationSegment = TextUtilities.GetLeadingWhitespace(document, line);
                                document.Replace(indentationSegment.Offset, indentationSegment.Length, sb.ToString(),
                                    OffsetChangeMappingType.RemoveAndInsert);
                            }
                        }
                    }
                    catch { }
                }
            }
        }
    }
}