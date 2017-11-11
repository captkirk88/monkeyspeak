using Monkeyspeak.lexical;
using Monkeyspeak.lexical.Expressions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;

namespace Monkeyspeak
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Monkeyspeak.lexical.AbstractParser" />
    public sealed class Parser : AbstractParser
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

        /// <summary>
        /// Parses the specified lexer's tokens.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns></returns>
        /// <exception cref="MonkeyspeakException">
        /// </exception>
        /// <exception cref="Exception">String length limit exceeded.</exception>
        public override IEnumerable<TriggerBlock> Parse(AbstractLexer lexer)
        {
            var block = new TriggerBlock(10);
            Trigger currentTrigger = Trigger.Undefined, prevTrigger = Trigger.Undefined;
            Token token = Token.None, prevToken = Token.None, nextToken = Token.None;
            Expression expr = null;
            foreach (var t in lexer.Read())
            {
                token = t;
                if (token == Token.None) continue;
                if (VisitToken != null)
                    token = VisitToken(ref token);

                var sourcePos = token.Position;

                string value = token.GetValue(lexer);

                //Logger.Debug<Parser>(token);
                switch (token.Type)
                {
                    case TokenType.TRIGGER:
                        if (currentTrigger != Trigger.Undefined && currentTrigger.Category != TriggerCategory.Undefined)
                        {
                            if (prevTrigger != Trigger.Undefined)
                            {
                                if (prevTrigger.Category == TriggerCategory.Effect && currentTrigger.Category == TriggerCategory.Cause)
                                {
                                    yield return block;
                                    prevTrigger = Trigger.Undefined;
                                    block = new TriggerBlock(10);
                                }

                                if (expr != null)
                                {
                                    currentTrigger.contents.Add(expr);
                                    expr = null;
                                }
                            }
                            block.Add(currentTrigger);
                            prevTrigger = currentTrigger;
                            currentTrigger = Trigger.Undefined;
                        }
                        if (string.IsNullOrWhiteSpace(value)) continue;
                        var cat = value.Substring(0, value.IndexOf(':'));
                        if (string.IsNullOrWhiteSpace(cat)) continue;
                        var id = value.Substring(value.IndexOf(':') + 1);
                        if (string.IsNullOrWhiteSpace(id)) continue;
                        currentTrigger = new Trigger((TriggerCategory)IntParse(cat),
                            IntParse(id), sourcePos);
                        break;

                    case TokenType.VARIABLE:
                        if (currentTrigger == Trigger.Undefined) throw new MonkeyspeakException($"Trigger was null. \nPrevious trigger = {prevTrigger}\nToken = {token}");
                        expr = new VariableExpression(ref sourcePos, value);
                        break;

                    case TokenType.TABLE:
                        if (currentTrigger == Trigger.Undefined) throw new MonkeyspeakException($"Trigger was null. \nPrevious trigger = {prevTrigger}\nToken = {token}");
                        expr = new VariableTableExpression(ref sourcePos, value.Substring(0, value.IndexOf('[')), value.Substring(value.IndexOf('[') + 1).TrimEnd(']'));
                        break;

                    case TokenType.STRING_LITERAL:
                        if (value.Length > Engine.Options.StringLengthLimit) throw new Exception("String length limit exceeded.");
                        if (currentTrigger == Trigger.Undefined) throw new MonkeyspeakException($"Trigger was null. \nPrevious trigger = {prevTrigger}\nToken = {token}");
                        expr = new StringExpression(ref sourcePos, value);
                        break;

                    case TokenType.NUMBER:
                        double val = double.Parse(value, System.Globalization.NumberStyles.AllowDecimalPoint
                            | System.Globalization.NumberStyles.AllowLeadingSign
                            | System.Globalization.NumberStyles.AllowExponent);
                        if (currentTrigger == Trigger.Undefined) throw new MonkeyspeakException($"Trigger was null. \nPrevious trigger = {prevTrigger}\nToken = {token}");
                        expr = new NumberExpression(ref sourcePos, val);
                        break;

                    case TokenType.COMMENT:
                        // we don't care about comments
                        break;

                    case TokenType.PREPROCESSOR:
                        break;

                    case TokenType.END_OF_FILE:
                        if (currentTrigger != Trigger.Undefined && currentTrigger.Category != TriggerCategory.Undefined)
                        {
                            if (currentTrigger != Trigger.Undefined && expr != null)
                            {
                                currentTrigger.contents.Add(expr);
                                expr = null;
                            }
                            block.Add(currentTrigger);
                            prevTrigger = currentTrigger;
                            currentTrigger = Trigger.Undefined;
                            yield return block;
                        }
                        break;

                    default: break;
                }
                if (currentTrigger != Trigger.Undefined && expr != null)
                {
                    currentTrigger.contents.Add(expr);
                    expr = null;
                }
            }
        }

        /// <summary>
        /// Ints the parse. (I love GhostDoc lol)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private int IntParse(string value)
        {
            int result = 0;
            for (int i = 0; i < value.Length; i++)
            {
                result = 10 * result + (value[i] - 48);
            }
            return result;
        }
    }
}