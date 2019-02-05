using System;
using System.Collections.Generic;
using Newtonsoft.Json;
// ReSharper disable UnusedMember.Global

namespace SoftwarePioniere.Messaging.Notifications
{
    /// <summary>
    /// Notification about as Failed Command
    /// </summary>
    public class CommandFailedNotification : NotificationContentBase
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const string TypeKey = "command.failed";

        public CommandFailedNotification() : base(TypeKey)
        {

        }

        [JsonProperty("command_id")]
        public Guid CommandId { get; set; }

        [JsonProperty("command_type")]
        public string CommandType { get; set; }

        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("trace_identifier")]
        public string TraceIdentifier { get; set; }

        [JsonProperty("object_type")]
        public string ObjectType { get; set; }

        [JsonProperty("object_id")]
        public string ObjectId { get; set; }

        [JsonProperty("error_text")]
        public string ErrorText { get; set; }

        public static NotificationMessage Create(ICommand cmd, Exception ex, IDictionary<string, string> state)
        {
            return new CommandFailedNotification()
            {
                CommandId = cmd.Id,
                CommandType = cmd.GetType().Name,
                RequestId = state.GetRequestId(),
                TraceIdentifier = state.GetTraceIdentifier(),
                ObjectId = cmd.ObjectId,
                ObjectType = cmd.ObjectType,
                ErrorText = ex.Message
            }.CreateNotificationMessage(cmd);
        }
    }
}