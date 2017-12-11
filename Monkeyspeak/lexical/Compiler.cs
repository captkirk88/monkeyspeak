using Monkeyspeak.Lexical.Expressions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Monkeyspeak.Lexical
{
    internal class Compiler
    {
        private Version version;

        public Compiler(MonkeyspeakEngine engine)
        {
            version = engine.Options.Version;
        }

        /// <summary>
        /// Compiler version number
        /// </summary>
        public Version Version
        {
            get { return version; }
        }

        private Trigger[] ReadVersion7_0(BinaryReader reader)
        {
            var sourcePos = new SourcePosition();

            Trigger[] triggers = new Trigger[reader.ReadInt32()];
            for (int i = 0; i <= triggers.Length - 1; i++)
            {
                var trigger = new Trigger((TriggerCategory)reader.ReadByte(), reader.ReadInt32(), sourcePos);

                int triggerContentCount = reader.ReadInt32();
                for (int k = 0; k <= triggerContentCount - 1; k++)
                {
                    var tokenType = (TokenType)reader.ReadByte();
                    if (!Expressions.Expressions.Instance.ContainsKey(tokenType))
                        throw new MonkeyspeakException($"Token {tokenType} does not have a expression associated with it");
                    var type = Expressions.Expressions.Instance[tokenType];
                    var content = (IExpression)Activator.CreateInstance(type);
                    content.Read(reader);
                    trigger.contents.Add(content);
                }
                triggers[i] = trigger;
                sourcePos = new SourcePosition(sourcePos.Line + 1, sourcePos.Column, sourcePos.RawPosition);
            }
            return triggers;
        }

        public Trigger[] DecompileFromStream(Stream stream)
        {
            Trigger[] triggers = null;
            //using (var decompressed = new DeflateStream(stream, CompressionMode.Decompress))
            using (var reader = new BinaryReader(stream))
            {
                var fileVersion = new Version(reader.ReadInt32(), reader.ReadInt32()); // use for versioning comparison
                switch (fileVersion.Major)
                {
                    case 1:
                        throw new MonkeyspeakException("Version 1 is a incompatible version.");

                    case 6:
                    case 7:
                    default:
                        triggers = ReadVersion7_0(reader);
                        break;
                }
            }
            return triggers;
        }

        public void CompileToStream(List<TriggerBlock> triggerBlocks, Stream stream)
        {
            //using (var compressed = new DeflateStream(stream, CompressionMode.Compress))
            Trigger[] triggers = triggerBlocks.SelectMany(block => block.ToArray()).ToArray();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(version.Major);
                writer.Write(version.Minor);

                writer.Write(triggers.Length);
                for (int i = 0; i <= triggers.Length - 1; i++)
                {
                    Trigger trigger = triggers[i];
                    writer.Write((byte)trigger.Category);
                    writer.Write(trigger.Id);

                    var count = trigger.contents.Count;
                    writer.Write(count);
                    for (int k = 0; k <= count - 1; k++)
                    {
                        var content = trigger.contents[k];
                        if (content.GetType().GetConstructor(Type.EmptyTypes) == null)
                            throw new MonkeyspeakException($"{content.GetType().Name} cannot be compiled, no parameter-less constructor");
                        var tokenType = Expressions.Expressions.GetTokenTypeFor(content.GetType());
                        if (tokenType == null)
                            writer.Write((byte)0);
                        else writer.Write((byte)tokenType);
                        content.Write(writer);
                    }
                }
                writer.Flush();
            }
        }
    }

    public interface ICompilable
    {
        void Write(BinaryWriter writer);

        void Read(BinaryReader reader);
    }
}