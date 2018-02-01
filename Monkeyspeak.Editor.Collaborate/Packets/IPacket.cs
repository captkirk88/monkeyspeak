using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Collaborate.Packets
{
    public enum PacketType : byte
    {
        None, AddText, RemoveText, Join, Leave
    }

    public interface IPacket
    {
        PacketType Type { get; }

        void Write(BinaryWriter writer);

        void Read(BinaryReader reader);
    }
}