using Monkeyspeak.Lexical;
using Monkeyspeak.Lexical.Expressions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;

namespace Monkeyspeak
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Monkeyspeak.Lexical.AbstractParser" />
    public class Parser : AbstractParser
    {
        public TokenVisitorHandler VisitToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        public Parser(MonkeyspeakEngine engine) :
            base(engine)
        {
        }

        public virtual void VisitExpression(Expression expr)
        {
        }

        /// <summary>
        /// Parses the specified lexer's tokens.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns></returns>
        /// <exception cref="MonkeyspeakException">
        /// </exception>
        /// <exception cref="Exception">String length limit exceeded.</exception>
        public override IEnumerable<Trigger> Parse(AbstractLexer lexer)
        {
            Trigger currentTrigger = Trigger.Undefined, lastTrigger = Trigger.Undefined;
            Token token = Token.None, prevToken = default(Token), nextToken = default(Token);
            IExpression expr = null;
            foreach (var t in lexer.Read())
            {
                token = t;
                var tokenType = token.Type;

                if (!Expressions.Instance.ContainsKey(tokenType)) continue;

                if (token == default(Token)) continue;
                if (VisitToken != null)
                    token = VisitToken(ref token);

                var sourcePos = token.Position;

                string value = token.GetValue(lexer);

                //Logger.Debug<Parser>(token);
                switch (tokenType)
                {
                    case TokenType.TRIGGER:
                        if (currentTrigger != Trigger.Undefined)
                        {
                            if (expr != null) expr.Apply(currentTrigger);
                            yield return currentTrigger;
                            lastTrigger = currentTrigger;
                            currentTrigger = Trigger.Undefined;
                        }
                        expr = Expressions.Create(tokenType, sourcePos, value);
                        lastTrigger = currentTrigger;
                        currentTrigger = expr.GetValue<Trigger>();
                        break;

                    case TokenType.VARIABLE:
                    case TokenType.TABLE:
                        expr = Expressions.Create(tokenType, sourcePos, value);
                        break;

                    case TokenType.STRING_LITERAL:
                        if (value.Length > Engine.Options.StringLengthLimit) throw new MonkeyspeakException("String length limit exceeded.");
                        expr = Expressions.Create(tokenType, sourcePos, value);
                        break;

                    case TokenType.NUMBER:
                        double val = double.Parse(value, System.Globalization.NumberStyles.AllowDecimalPoint
                            | System.Globalization.NumberStyles.AllowLeadingSign
                            | System.Globalization.NumberStyles.AllowExponent);
                        expr = Expressions.Create(tokenType, sourcePos, val);
                        break;

                    case TokenType.COMMENT:
                        // we don't care about comments
                        break;

                    case TokenType.PREPROCESSOR:
                        break;

                    case TokenType.END_OF_FILE:
                        if (currentTrigger != Trigger.Undefined)
                        {
                            expr.Apply(currentTrigger);
                            yield return currentTrigger;
                            lastTrigger = currentTrigger;
                            currentTrigger = Trigger.Undefined;
                        }
                        break;

                    default: break;
                }
                if (expr != null)
                {
                    if (currentTrigger != Trigger.Undefined)
                    {
                        expr.Apply(currentTrigger);
                    }
                    expr = null;
                }
            }
            if (currentTrigger != Trigger.Undefined)
            {
                yield return currentTrigger;
            }
        }
    }
}