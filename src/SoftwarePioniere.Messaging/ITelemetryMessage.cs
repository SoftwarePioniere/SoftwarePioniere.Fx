using System.Collections.Generic;

namespace SoftwarePioniere.Messaging
{
    public interface ITelemetryMessage : IMessage
    {
        string MessageType { get; }

        string MessageContent { get; }

        IDictionary<string, string> Properties { get; }
    }

    public interface ITypedTelemetryMessage<out T> : IMessage
    {
        string MessageType { get; }

        T MessageContent { get; }

        IDictionary<string, string> Properties { get; }
    }
}