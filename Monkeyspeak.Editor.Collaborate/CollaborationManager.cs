using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Extensions;
using System.IO;
using Monkeyspeak.Editor.Collaborate.Packets;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Editor.Collaborate
{
    public class CollaborationManager
    {
        private static bool isOwner = false, connected = false;

        private static List<Collaborator> collaborators = new List<Collaborator>();

        public static bool IsOwner { get => isOwner; set => isOwner = value; }

        public static event Action<Collaborator> Added, Removed;

        public static void Create(IEditor editor)
        {
            throw new NotImplementedException("patience!");
        }

        public static bool Open(IEditor editor, string joinCode)
        {
            throw new NotImplementedException("patience!");
        }

        private static void OnMessage(object sender, byte[] data, IEditor editor)
        {
            var mem = new MemoryStream(data);
            var reader = new BinaryReader(mem);
            IPacket packet = null;
            var type = (PacketType)reader.ReadByte();
            switch (type)
            {
                case PacketType.Join:
                    packet = new RequestToJoin();
                    break;
            }

            if (packet != null) packet.Read(reader);
        }

        public static void Send(IEditor editor, IPacket packet)
        {
            throw new NotImplementedException("patience!");
        }

        public static void Disconnect(Collaborator collab)
        {
            throw new NotImplementedException("patience!");
        }

        public static void Shutdown(IEditor editor)
        {
            throw new NotImplementedException("patience!");
        }

        private static int GetFreePort()
        {
            var l = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            l.Start();
            int port = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}