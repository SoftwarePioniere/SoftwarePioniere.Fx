using System;
using System.Collections.Generic;

namespace SoftwarePioniere.Messaging
{
  
    public class WrappedMessage<T> : MessageBase, IMessageWrapper<T>
        where T : IMessage
    {
        public string MessageType { get; }

        public T MessageContent { get; }
 
        public IDictionary<string, string> Properties { get; }

        public WrappedMessage(Guid id, DateTime timeStampUtc, string userId, T messageContent, IDictionary<string, string> properties) : base(id, timeStampUtc, userId)
        {
            MessageType = typeof(T).GetTypeShortName();
            MessageContent = messageContent;
            Properties = properties;
        }
    }

    public static class WrappedMessageExtensions
    {
        public static WrappedMessage<T> CreateWrappedMessage<T>(this T message, IDictionary<string, string> state)
            where T : IMessage
        {
            var tm = new WrappedMessage<T>(message.Id, message.TimeStampUtc, message.UserId
                , message
                , state
            );

            return tm;
        }

    }

}