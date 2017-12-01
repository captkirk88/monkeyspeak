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
    public sealed class VariableTableExpression : VariableExpression
    {
        public VariableTableExpression(SourcePosition pos, string varRef) : base(pos, varRef.Substring(0, varRef.IndexOf('[')))
        {
            Indexer = varRef.Substring(varRef.IndexOf('[') + 1).TrimEnd(']');
        }

        public VariableTableExpression()
        {
        }

        public string Indexer { get; private set; }
        public bool HasIndex { get => string.IsNullOrWhiteSpace(Indexer); }

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
                var var = VariableTable.Empty;
                string varRef = GetValue<string>();
                if (!page.HasVariable(varRef, out var))
                    if (addToPage)
                    {
                        var = page.CreateVariableTable(varRef, false);
                    }
                if (HasIndex) var.ActiveIndexer = Indexer;
                return var as VariableTable ?? VariableTable.Empty;
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