using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Utils
{
    /// <summary>
    /// Reads triggers from a stream and updates those trigger's to reflect the library's trigger description.
    /// This is useful if you have a trigger handler that was changed but it isn't reflected on the script itself.
    ///
    /// Caution: This is a intensive operation due to analyzing and replacing any characters in the code that don't match up.
    /// Use sparingly.
    /// </summary>
    public sealed class TriggerIntegrityUpdater
    {
        private MonkeyspeakEngine engine;

        public TriggerIntegrityUpdater(MonkeyspeakEngine engine)
        {
            this.engine = engine;
        }

        public string UpdateTriggers(string code)
        {
            throw new NotImplementedException();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(code)))
            using (var reader = new SStreamReader(stream))
            using (var lexer = new Lexer(engine, reader))
                foreach (var token in lexer.Read())
                {
                    if (token.Type == TokenType.TRIGGER)
                    {
                    }
                }
            return null;
        }
    }
}