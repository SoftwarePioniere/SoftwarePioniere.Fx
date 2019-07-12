using System.Collections.Generic;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public class MessageWrapper : IMessageWrapper
    {
        public MessageWrapper(string messageType, string messageContent, IDictionary<string, string> properties)
        {
            MessageType = messageType;
            MessageContent = messageContent;
            Properties = properties;
        }

        public string MessageType { get; }
        public string MessageContent { get; }
        public IDictionary<string, string> Properties { get; }
    }


    public static class MessageWrapperExtensions
    {
      
        public static MessageWrapper CreateMessageWrapper(this IMessage message, IDictionary<string, string> state)
        {
            return new MessageWrapper(message.GetType().GetTypeShortName(),
                JsonConvert.SerializeObject(message), state);
        }

        public static bool IsWrappedType<T>(this MessageWrapper item)
        {
            return typeof(T).GetTypeShortName().Equals(item.MessageType);
        }

        public static T GetWrappedMessage<T>(this MessageWrapper item)
        where T : class
        {
            if (item.IsWrappedType<T>())
            {
                return JsonConvert.DeserializeObject<T>(item.MessageContent);
            }
            return default(T);
        }


    }
}
