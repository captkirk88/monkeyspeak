using Monkeyspeak.Lexical.Expressions;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

using System.Reflection;
using System.Linq.Expressions;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Lexical.Expressions
{
    public sealed class Expressions : Dictionary<TokenType, Type>
    {
        #region Statics

        public static readonly Expressions Instance = new Expressions()
        {
            {TokenType.NUMBER, typeof(NumberExpression) },
            {TokenType.STRING_LITERAL, typeof(StringExpression) },
            {TokenType.TRIGGER, typeof(TriggerExpression) },
            {TokenType.VARIABLE, typeof(VariableExpression) },
            {TokenType.TABLE, typeof(VariableTableExpression) }
        };

        private static Dictionary<Type, ObjectCreation.Creator> typeCache = new Dictionary<Type, ObjectCreation.Creator>();

        public static TokenType? GetTokenTypeFor(Type exprType)
        {
            var tokenType = Instance.Where(kv => kv.Value == exprType).Select(kv => kv.Key).FirstOrDefault();
            if (tokenType == default(TokenType))
                return null;
            return tokenType;
        }

        public static TokenType? GetTokenTypeFor<T>()
        {
            var tokenType = Instance.Where(kv => kv.Value == typeof(T)).Select(kv => kv.Key).FirstOrDefault();
            if (tokenType == default(TokenType))
                return null;
            return tokenType;
        }

        internal static IExpression Create<T>(TokenType tokenType, SourcePosition pos, T value)
        {
            var exprType = Instance[tokenType];
            if (typeCache.ContainsKey(exprType))
            {
                return (IExpression)typeCache[exprType](pos, value);
            }
            else
            {
                var creator = ObjectCreation.GetCreator(exprType, new Type[] { typeof(SourcePosition), typeof(T) });
                typeCache.Add(exprType, creator);
                return (IExpression)creator(pos, value);
            }

            //return (IExpression)Activator.CreateInstance(Instance[tokenType], pos, value);
        }

        internal static IExpression Create(TokenType tokenType, SourcePosition pos, object value)
        {
            var exprType = Instance[tokenType];
            if (typeCache.ContainsKey(exprType))
            {
                return (IExpression)typeCache[exprType](pos, value);
            }
            else
            {
                var creator = ObjectCreation.GetCreator(exprType, new Type[] { typeof(SourcePosition), value.GetType() });
                typeCache.Add(exprType, creator);
                return (IExpression)creator(pos, value);
            }

            //return (IExpression)Activator.CreateInstance(Instance[tokenType], pos, value);
        }

        #endregion Statics

        public void Replace<T>(TokenType type) where T : IExpression
        {
            this[type] = typeof(T);
        }
    }
}