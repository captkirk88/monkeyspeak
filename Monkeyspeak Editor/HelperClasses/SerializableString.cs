using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.HelperClasses
{
    [Serializable]
    public class SerializableString : ISerializable
    {
        private string content;

        public SerializableString()
        {
        }

        public SerializableString(string content)
        {
            this.Content = content;
        }

        public string Content { get => content; set => content = value; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("content", Content);
        }

        public static implicit operator SerializableString(string content)
        {
            return new SerializableString(content);
        }

        public static implicit operator string(SerializableString str)
        {
            return str.content;
        }
    }
}