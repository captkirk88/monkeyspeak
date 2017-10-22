using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.lexical.TokenDefinitions
{
    internal class VariableDefinition : ITokenDefinition
    {
        public TokenType Type => TokenType.VARIABLE;

        public char StartCharacter { get; private set; }

        public VariableDefinition(char varChar)
        {
            StartCharacter = varChar;
        }

        public Token Create(AbstractLexer lexer, SStreamReader reader)
        {
            long startPos = reader.Position;
            int length = 0;
            int currentChar = lexer.Next();
            length++;
            var sourcePos = lexer.CurrentSourcePosition;

            lexer.CheckMatch(StartCharacter);

            currentChar = lexer.Next();
            length++;
            while (true)
            {
                if (!((currentChar >= 'a' && currentChar <= 'z')
                   || (currentChar >= 'A' && currentChar <= 'Z')
                   || (currentChar >= '0' && currentChar <= '9')
                   || currentChar == '_'))
                {
                    length--;
                    break;
                }
                currentChar = lexer.Next();
                length++;
            }

            if (currentChar == -1)
            {
                throw new MonkeyspeakException("Unexpected end of file", sourcePos);
            }

            #region Variable Table Handling

            if (lexer.LookAhead(1) == '[')
            {
                while (((currentChar >= 'a' && currentChar <= 'z')
                        || (currentChar >= 'A' && currentChar <= 'Z')
                        || (currentChar >= '0' && currentChar <= '9')))
                {
                    if (currentChar == -1)
                    {
                        throw new MonkeyspeakException("Unexpected end of file", lexer.CurrentSourcePosition);
                    }
                    currentChar = lexer.Next();
                    length++;
                    if (currentChar == ']')
                    {
                        break;
                    }
                    if (!((currentChar >= 'a' && currentChar <= 'z')
                        || (currentChar >= 'A' && currentChar <= 'Z')
                        || (currentChar >= '0' && currentChar <= '9')))
                    {
                        throw new MonkeyspeakException($"Invalid character in variable list index delcaration '{currentChar}'", lexer.CurrentSourcePosition);
                    }
                }
            }

            #endregion Variable Table Handling

            return new Token(TokenType.VARIABLE, startPos, length, lexer.CurrentSourcePosition);
        }
    }
}