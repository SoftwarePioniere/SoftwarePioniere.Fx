using System;
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

        [JsonProperty("error_text")]
        public string ErrorText { get; set; }

        public static NotificationMessage Create(ICommand cmd, Exception ex)
        {
            return new CommandFailedNotification()
            {
                CommandId = cmd.Id,
                CommandType = cmd.GetType().Name,
                RequestId = cmd.RequestId,            
                ErrorText = ex.Message
            }.CreateNotificationMessage(cmd);
        }
    }
}