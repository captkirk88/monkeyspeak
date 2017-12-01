using System.Collections.Generic;

namespace Monkeyspeak.Lexical
{
    public abstract class AbstractParser
    {
        protected MonkeyspeakEngine Engine;

        protected AbstractParser(MonkeyspeakEngine engine)
        {
            Engine = engine;
        }

        public string[] PreprocessorDefines { get; set; }

        public abstract IEnumerable<Trigger> Parse(AbstractLexer lexer);
    }
}