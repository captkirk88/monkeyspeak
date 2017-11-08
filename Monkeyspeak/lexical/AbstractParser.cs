using System.Collections.Generic;

namespace Monkeyspeak.lexical
{
    public abstract class AbstractParser
    {
        protected MonkeyspeakEngine Engine;

        protected AbstractParser(MonkeyspeakEngine engine)
        {
            Engine = engine;
        }

        public string[] PreprocessorDefines { get; set; }

        public abstract IEnumerable<TriggerBlock> Parse(AbstractLexer lexer);
    }
}