using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peer2Net;

namespace Monkeyspeak.Editor.Collaborate
{
    internal class CollaboratingTcpListener : TcpListener
    {
        private Guid guid;

        public CollaboratingTcpListener(int port) : base(port)
        {
            guid = Guid.NewGuid();
        }

        public Guid Guid { get => guid; }
    }
}