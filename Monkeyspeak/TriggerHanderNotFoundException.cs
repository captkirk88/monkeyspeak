using System;
using System.Runtime.Serialization;

namespace Monkeyspeak
{
    [Serializable]
    internal class TriggerHanderNotFoundException : Exception
    {
        public TriggerHanderNotFoundException()
        {
        }

        public TriggerHanderNotFoundException(string message) : base(message)
        {
        }

        public TriggerHanderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TriggerHanderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}