using Monkeyspeak.lexical;
using System;
using System.Collections.Generic;

namespace Monkeyspeak
{
    public abstract class AbstractLexer : IDisposable
    {
        protected readonly SStreamReader reader;

        protected AbstractLexer(MonkeyspeakEngine engine, SStreamReader reader)
        {
            this.reader = reader;
            Engine = engine;
        }

        /// <summary>
        /// Gets the engine.
        /// </summary>
        /// <value>
        /// The engine.
        /// </value>
        public virtual MonkeyspeakEngine Engine { get; private set; }

        /// <summary>
        /// Gets the current source position.
        /// </summary>
        /// <value>
        /// The current source position.
        /// </value>
        public virtual SourcePosition CurrentSourcePosition
        {
            get;
        }

        /// <summary>
        /// Advances one character in the reader.
        /// </summary>
        public abstract int Next();

        /// <summary>
        /// Reads the tokens from the reader.  Used by the Parser.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<Token> Read();

        /// <summary>
        /// Reads the specified start position in stream.  Used by the Token to read the token's value.
        /// </summary>
        /// <param name="startPosInStream">The start position in stream.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public abstract char[] Read(long startPosInStream, int length);

        /// <summary>
        /// Looks ahead a specified number of chars and returns the result.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public abstract int LookAhead(int amount);

        /// <summary>
        /// Looks back a specified number of chars and returns the result.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public abstract int LookBack(int amount);

        /// <summary>
        /// Checks if the next input is a match to <paramref name="c"/>.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <exception cref="MonkeyspeakException" />
        public abstract void CheckMatch(int c);

        /// <summary>
        /// Checks if the next input is a match to <paramref name="c"/>.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <exception cref="MonkeyspeakException" />
        public abstract void CheckMatch(char c);

        /// <summary>
        /// Checks if the next input is a match to <paramref name="str"/>.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <exception cref="MonkeyspeakException" />
        public abstract void CheckMatch(string str);

        /// <summary>
        /// Checks if <paramref name="c"/> is a number.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <exception cref="MonkeyspeakException" />
        public abstract void CheckIsDigit(char c = '\0');

        /// <summary>
        /// Checks if the next input results in End of File.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <exception cref="MonkeyspeakException" />
        public abstract void CheckEOF(int c);

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            reader.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}