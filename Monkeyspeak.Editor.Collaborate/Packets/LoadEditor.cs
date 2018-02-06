using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Collaborate.Packets
{
    public sealed class LoadEditor : IPacket
    {
        public LoadEditor()
        {
        }

        public LoadEditor(IEditor editor)
        {
            FileName = Path.GetFileName(editor.CurrentFilePath);
            Text = editor.Text;
        }

        public PacketType Type => PacketType.LoadEditor;

        public string FileName { get; private set; }
        public string Text { get; private set; }

        public void Read(BinaryReader reader)
        {
            FileName = reader.ReadString();
            Text = reader.ReadString();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FileName);
            writer.Write(Text);
        }
    }
}