using System;
using System.Collections.Generic;

using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace SoftwarePioniere.Messaging.Notifications
{
    /// <summary>
    /// Notiification about a Succeeded Command
    /// </summary>
    public class CommandSucceededNotification : NotificationContentBase
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const string TypeKey = "command.succeeded";


        /// <inheritdoc />
        public CommandSucceededNotification() : base(TypeKey)
        {
        }

        [J("command_id")]
        [J1("command_id")]
        public Guid CommandId { get; set; }

        [J("command_type")]
        [J1("command_type")]
        public string CommandType { get; set; }

        [J("request_id")]
        [J1("request_id")]
        public string RequestId { get; set; }

        [J("trace_identifier")]
        [J1("trace_identifier")]
        public string TraceIdentifier { get; set; }

        [J("object_type")]
        [J1("object_type")]
        public string ObjectType { get; set; }

        [J("object_id")]
        [J1("object_id")]
        public string ObjectId { get; set; }

        [J("user_id")]
        [J1("user_id")]
        public string UserId { get; set; }

        // ReSharper disable once UnusedMember.Global
        public static NotificationMessage Create(ICommand cmd, Dictionary<string, string> state)
        {
            return new CommandSucceededNotification
            {
                CommandId = cmd.Id,
                CommandType = cmd.GetType().Name,
                RequestId = state.GetRequestId(),
                TraceIdentifier = state.GetTraceIdentifier(),
                ObjectId = cmd.ObjectId,
                ObjectType = cmd.ObjectType,
                UserId = cmd.UserId
            }.CreateNotificationMessage(cmd);
        }
    }
}