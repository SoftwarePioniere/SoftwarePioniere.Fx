using System;
using Newtonsoft.Json;

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

        [JsonProperty("command_id")]
        public Guid CommandId { get; set; }

        [JsonProperty("command_type")]
        public string CommandType { get; set; }

        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        // ReSharper disable once UnusedMember.Global
        public static NotificationMessage Create(ICommand cmd)
        {
            return new CommandSucceededNotification
            {
                CommandId = cmd.Id,
                CommandType = cmd.GetType().Name,
                RequestId = cmd.RequestId               
            }.CreateNotificationMessage(cmd);
        }
    }
}