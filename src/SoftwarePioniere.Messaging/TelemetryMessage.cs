﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public class TelemetryMessage : MessageBase, ITelemetryMessage
    {
        public string MessageType { get; }
        public string MessageContent { get; }
        public IDictionary<string, string> Properties { get; }

        public TelemetryMessage(Guid id, DateTime timeStampUtc, string userId, string messageType, string messageContent, IDictionary<string, string> properties) : base(id, timeStampUtc, userId)
        {
            MessageType = messageType;
            MessageContent = messageContent;
            Properties = properties;
        }
    }

    public class TypedTelemetryMessage<T> : MessageBase, ITypedTelemetryMessage<T>
    {
        public string MessageType { get; }

        public T MessageContent { get; }

        //public string MessageContent { get; }
        public IDictionary<string, string> Properties { get; }

        public TypedTelemetryMessage(Guid id, DateTime timeStampUtc, string userId, T messageContent, IDictionary<string, string> properties) : base(id, timeStampUtc, userId)
        {
            MessageType = typeof(T).GetTypeShortName();// messageType;
            //MessageContent = messageContent;
            MessageContent = messageContent;
            Properties = properties;
        }
    }

    public static class TelemetryMessageExtensions
    {
        public static TypedTelemetryMessage<T> CreateTypedTelemetryMessage<T>(this T message, IDictionary<string, string> state)
            where T : IMessage
        {
            var tm = new TypedTelemetryMessage<T>(message.Id, message.TimeStampUtc, message.UserId
                , message
                , state
            );

            return tm;
        }

        public static TelemetryMessage CreateTelemetryMessage(this IMessage message, IDictionary<string, string> state)
        {
            var tm = new TelemetryMessage(message.Id, message.TimeStampUtc, message.UserId
            , message.GetType().GetTypeShortName(),
            JsonConvert.SerializeObject(message, Formatting.None)
            , state
                );

            return tm;
        }

        public static T Cast<T>(this TelemetryMessage message) where T : IMessage
        {
            return JsonConvert.DeserializeObject<T>(message.MessageContent);
        }

        public static Dictionary<string, object> CreateLoggerScope(this IDictionary<string, string> state)
        {
            var dict = new Dictionary<string, object>();
            foreach (var key in state.Keys)
            {
                dict.Add(key, state[key]);
            }
            return dict;
        }
    }






}
