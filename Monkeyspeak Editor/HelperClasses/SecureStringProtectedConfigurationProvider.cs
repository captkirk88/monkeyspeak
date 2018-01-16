using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Monkeyspeak.Extensions;

namespace Monkeyspeak.Editor.HelperClasses
{
    internal class SecureStringProtectedConfigurationProvider : ProtectedConfigurationProvider
    {
        public override XmlNode Decrypt(XmlNode encryptedNode)
        {
            encryptedNode.Value = encryptedNode.Value.DecryptString().ToInsecureString();
            return encryptedNode;
        }

        public override XmlNode Encrypt(XmlNode node)
        {
            node.Value = node.Value.ToSecureString().EncryptString();
            return node;
        }
    }
}