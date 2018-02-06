using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Collaborate.Packets
{
    public sealed class ChatMessage : IPacket
    {
        public ChatMessage()
        {
        }

        public ChatMessage(string text)
        {
            Text = text;
        }

        public PacketType Type => throw new NotImplementedException();

        public string Text { get; private set; }

        public void Read(BinaryReader reader)
        {
            Text = reader.ReadString();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Text);
        }
    }
}