using System;
using System.Collections.Generic;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;
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

        [J("error_text")]
        [J1("error_text")]
        public string ErrorText { get; set; }

        [J("user_id")]
        [J1("user_id")]
        public string UserId { get; set; }

        public static NotificationMessage Create(ICommand cmd, Exception ex, Dictionary<string, string> state)
        {
            return new CommandFailedNotification()
            {
                CommandId = cmd.Id,
                CommandType = cmd.GetType().Name,
                RequestId = state.GetRequestId(),
                TraceIdentifier = state.GetTraceIdentifier(),
                ObjectId = cmd.ObjectId,
                ObjectType = cmd.ObjectType,
                ErrorText = ex.Message,
                UserId = cmd.UserId
            }.CreateNotificationMessage(cmd);
        }
    }
}