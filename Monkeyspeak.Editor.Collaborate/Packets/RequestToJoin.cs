using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Collaborate.Packets
{
    internal class RequestToJoin : IPacket
    {
        public RequestToJoin()
        {
        }

        public RequestToJoin(IEditor editor)
        {
            EditorTitle = editor.Title;
        }

        public string EditorTitle { get; set; }

        public PacketType Type => PacketType.Join;

        public void Read(BinaryReader reader)
        {
            EditorTitle = reader.ReadString();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(EditorTitle);
        }
    }
}