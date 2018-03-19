using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Lexical.Expressions
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Monkeyspeak.Lexical.Expressions.VariableExpression"/>
    public sealed class VariableTableExpression : VariableExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableTableExpression"/> class.
        /// </summary>
        /// <param name="pos">   The position.</param>
        /// <param name="varRef">The variable reference.</param>
        public VariableTableExpression(SourcePosition pos, string varRef) : base(pos, varRef.Substring(0, varRef.IndexOf('[')))
        {
            Indexer = varRef.RightOf('[').LeftOf(']');
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableTableExpression"/> class.
        /// </summary>
        public VariableTableExpression()
        {
        }

        public string Indexer { get; private set; }
        public bool HasIndex { get => !string.IsNullOrWhiteSpace(Indexer); }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(GetValue<string>());
            writer.Write(HasIndex);
            if (HasIndex)
                writer.Write(Indexer);
        }

        public override void Read(BinaryReader reader)
        {
            SetValue(reader.ReadString());
            Indexer = reader.ReadBoolean() ? reader.ReadString() : null;
        }

        public override object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
        {
            try
            {
                string varRef = GetValue<string>();
                if (!page.HasVariable(varRef, out IVariable var))
                    if (addToPage)
                    {
                        var = page.CreateVariableTable(varRef, false);
                    }
                if (HasIndex)
                {
                    if (var is VariableTable table)
                    {
                        table.ActiveIndexer = Indexer;
                        return table;
                    }
                }
                return var ?? VariableTable.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error<TriggerReader>(ex);
                return VariableTable.Empty;
            }
        }

        public override string ToString()
        {
            return $"{GetValue<string>()}{(HasIndex ? '[' + Indexer + ']' : string.Empty)} {Position}";
        }
    }
}