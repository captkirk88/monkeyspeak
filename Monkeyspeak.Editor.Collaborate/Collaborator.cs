using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peer2Net;

namespace Monkeyspeak.Editor.Collaborate
{
    public sealed class Collaborator : IEquatable<Collaborator>
    {
        private Peer peer;

        private IEditor editor;

        public Collaborator(Peer peer, IEditor editor)
        {
            this.peer = peer;
            this.editor = editor;
        }

        public Peer Peer { get => peer; }
        public IEditor Editor { get => editor; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Collaborator);
        }

        public bool Equals(Collaborator other)
        {
            return other != null &&
                   EqualityComparer<Peer>.Default.Equals(peer, other.peer);
        }

        public override int GetHashCode()
        {
            return -1716539471 + EqualityComparer<Peer>.Default.GetHashCode(peer);
        }
    }
}