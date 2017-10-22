﻿using Monkeyspeak.lexical.Expressions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Monkeyspeak.lexical
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

        private IEnumerable<TriggerBlock> ReadVersion6_5(BinaryReader reader)
        {
            var sourcePos = new SourcePosition();

            int triggerListCount = reader.ReadInt32();
            for (int i = 0; i <= triggerListCount - 1; i++)
            {
                var triggerList = new TriggerBlock();
                int triggerCount = reader.ReadInt32();
                for (int j = 0; j <= triggerCount - 1; j++)
                {
                    var trigger = new Trigger((TriggerCategory)reader.ReadInt32(), reader.ReadInt32());

                    int triggerContentCount = reader.ReadInt32();
                    if (triggerContentCount > 0)
                        for (int k = triggerContentCount - 1; k >= 0; k--)
                        {
                            byte type = reader.ReadByte();
                            switch (type)
                            {
                                case 1:
                                    trigger.contents.Add(new StringExpression(ref sourcePos, reader.ReadString()));
                                    break;

                                case 2:
                                    trigger.contents.Add(new NumberExpression(ref sourcePos, reader.ReadDouble()));
                                    break;

                                case 3:
                                    trigger.contents.Add(new VariableExpression(ref sourcePos, reader.ReadString()));
                                    break;

                                case 4: // reserved
                                    break;
                            }
                        }
                    triggerList.Add(trigger);
                }
                yield return triggerList;
            }
        }

        private IEnumerable<TriggerBlock> ReadVersion1(BinaryReader reader)
        {
            var sourcePos = new SourcePosition();

            int triggerListCount = reader.ReadInt32();
            for (int i = 0; i <= triggerListCount - 1; i++)
            {
                var triggerList = new TriggerBlock();
                int triggerCount = reader.ReadInt32();
                for (int j = 0; j <= triggerCount - 1; j++)
                {
                    var trigger = new Trigger((TriggerCategory)reader.ReadInt32(), reader.ReadInt32());

                    string description = reader.ReadString(); // no longer used, here for compatibility.
                    int triggerContentCount = reader.ReadInt32();
                    if (triggerContentCount > 0)
                        for (int k = 0; k <= triggerContentCount - 1; k++)
                        {
                            if (reader.ReadBoolean())
                            {
                                byte type = reader.ReadByte();
                                if (type == 1) // String
                                {
                                    trigger.contents.Add(new StringExpression(ref sourcePos, reader.ReadString()));
                                }

                                if (type == 2) // Double
                                {
                                    trigger.contents.Add(new NumberExpression(ref sourcePos, reader.ReadDouble()));
                                }
                            }
                        }
                    triggerList.Add(trigger);
                }
                yield return triggerList;
            }
        }

        public TriggerBlock[] DecompileFromStream(Stream stream)
        {
            TriggerBlock[] blocks = null;
            using (var decompressed = new DeflateStream(stream, CompressionMode.Decompress))
            using (var reader = new BinaryReader(decompressed, Encoding.UTF8, true))
            {
                var fileVersion = new Version(reader.ReadInt32(), reader.ReadInt32()); // use for versioning comparison
                switch (fileVersion.Major)
                {
                    case 1:
                        blocks = ReadVersion1(reader).ToArray();
                        break;

                    case 6:
                        blocks = ReadVersion6_5(reader).ToArray();
                        break;

                    default:
                        blocks = ReadVersion6_5(reader).ToArray();
                        break;
                }
            }
            return blocks;
        }

        public void CompileToStream(List<TriggerBlock> triggerBlocks, Stream stream)
        {
            using (var compressed = new DeflateStream(stream, CompressionMode.Compress))
            using (var writer = new BinaryWriter(compressed, Encoding.UTF8, true))
            {
                writer.Write(version.Major);
                writer.Write(version.Minor);

                writer.Write(triggerBlocks.Count);
                for (int i = 0; i <= triggerBlocks.Count - 1; i++)
                {
                    TriggerBlock triggerBlock = triggerBlocks[i];
                    writer.Write(triggerBlock.Count);
                    for (int j = 0; j <= triggerBlock.Count - 1; j++)
                    {
                        Trigger trigger = triggerBlock[j];
                        writer.Write((int)trigger.Category);
                        writer.Write(trigger.Id);

                        var count = trigger.contents.Count;
                        writer.Write(count);
                        for (int k = 0; k <= count - 1; k++)
                        {
                            var content = trigger.contents[k];
                            if (content is StringExpression)
                            {
                                writer.Write((byte)1);
                                writer.Write(((StringExpression)content).Value);
                            }
                            else if (content is NumberExpression)
                            {
                                writer.Write((byte)2);
                                writer.Write(((NumberExpression)content).Value);
                            }
                            else if (content is VariableExpression)
                            {
                                writer.Write((byte)3);
                                writer.Write(((VariableExpression)content).Value);
                            }
                            else writer.Write((byte)4); // reserved
                        }
                    }
                }
            }
        }
    }
}