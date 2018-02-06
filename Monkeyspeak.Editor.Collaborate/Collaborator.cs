using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Collaborate
{
    public sealed class Collaborator : IEquatable<Collaborator>
    {
        internal static Collaborator Self = new Collaborator(NetUtils.GetLocalEndPoint(), null);

        private IPEndPoint endPoint;

        private IEditor editor;

        private string mappedId;

        internal Collaborator(IPEndPoint endPoint, IEditor editor)
        {
            this.endPoint = endPoint;
            this.editor = editor;
            Name = Environment.UserName;
            var mac = string.Join(string.Empty, NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.OperationalStatus == OperationalStatus.Up && i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => Convert.ToBase64String(nic.GetPhysicalAddress().GetAddressBytes()))
                .ToArray());
            var ip = Convert.ToBase64String(endPoint.Address.GetAddressBytes());
            mappedId = string.Join(string.Empty, mac, ip);
        }

        internal IPEndPoint EndPoint { get => endPoint; }
        public IEditor Editor { get => editor; }

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public string UniqueId { get => mappedId; }

        public string Name { get; private set; }

        public void Kick()
        {
            CollaborationManager.Disconnect(this);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Collaborator);
        }

        public bool Equals(Collaborator other)
        {
            return other != null &&
                   EqualityComparer<IPEndPoint>.Default.Equals(endPoint, other.endPoint) &&
                   EqualityComparer<IEditor>.Default.Equals(editor, other.editor);
        }

        public override int GetHashCode()
        {
            var hashCode = -172150231;
            hashCode = hashCode * -1521134295 + EqualityComparer<IPEndPoint>.Default.GetHashCode(endPoint);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEditor>.Default.GetHashCode(editor);
            return hashCode;
        }
    }
}