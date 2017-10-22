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

        public abstract IEnumerable<TriggerBlock> Parse(AbstractLexer lexer);
    }
}