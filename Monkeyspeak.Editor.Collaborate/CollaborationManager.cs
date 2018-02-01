using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peer2Net;
using Monkeyspeak.Extensions;
using System.IO;
using Monkeyspeak.Editor.Collaborate.Packets;

namespace Monkeyspeak.Editor.Collaborate
{
    public class CollaborationManager
    {
        private static bool isOwner = false, connected = false;
        private static Dictionary<IEditor, CollaboratingTcpListener> listeners = new Dictionary<IEditor, CollaboratingTcpListener>();
        private static int port;
        private static Dictionary<IEditor, CommunicationManager> comms = new Dictionary<IEditor, CommunicationManager>();

        private static List<Collaborator> collaborators = new List<Collaborator>();

        public static bool IsOwner { get => isOwner; set => isOwner = value; }

        public static event Action<Collaborator> Added, Removed;

        public static void Initialize(IEditor editor)
        {
            collaborators = new List<Collaborator>();
            var listener = new CollaboratingTcpListener(GetFreePort());
            var comm = new CommunicationManager(listener);
            comm.ConnectionClosed += Comm_ConnectionClosed;
            comm.PeerConnected += Comm_PeerConnected;
            comm.PeerDataReceived += Comm_PeerDataReceived;
            comm.ConnectionFailed += Comm_ConnectionFailed;
            if (listeners.ContainsKey(editor) == false)
            {
                listeners.Add(editor, listener);
                comms.Add(editor, comm);
                editor.Closing += Shutdown;

                listener.Start();
            }
        }

        private static void Comm_PeerDataReceived(object sender, Peer2Net.EventArgs.PeerDataEventArgs e)
        {
            var mem = new MemoryStream(e.Data);
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

        public static void Shutdown(IEditor editor)
        {
            try
            {
                if (!comms.TryGetValue(editor, out var comm)) return;
                foreach (var collaborator in collaborators.Where(c => c.Editor == editor))
                {
                    comm.Disconnect(collaborator.Peer.EndPoint);
                }
                if (listeners.TryGetValue(editor, out var listener))
                    listener.Stop();
            }
            catch { }
        }

        private static void Comm_ConnectionFailed(object sender, Peer2Net.EventArgs.ConnectionEventArgs e)
        {
            RemoveCollaborator(e.EndPoint);
        }

        private static void Comm_PeerConnected(object sender, Peer2Net.EventArgs.PeerEventArgs e)
        {
            AddCollaborator(e.Peer);
        }

        private static void Comm_ConnectionClosed(object sender, Peer2Net.EventArgs.ConnectionEventArgs e)
        {
            RemoveCollaborator(e.EndPoint);
        }

        private static void AddCollaborator(Peer peer)
        {
            var collaborator = collaborators.Find(c => c.Peer.EndPoint == peer.EndPoint);
            if (collaborator == null) collaborators.Add(new Collaborator(peer));
        }

        private static void RemoveCollaborator(System.Net.IPEndPoint endPoint)
        {
            var collaborator = collaborators.FirstOrDefault(c => c.Peer.EndPoint == endPoint);
            if (collaborator != null)
            {
                collaborators.Remove(collaborator);
                Removed?.Invoke(collaborator);
            }
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