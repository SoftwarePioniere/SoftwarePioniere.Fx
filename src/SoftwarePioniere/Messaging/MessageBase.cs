using System;

namespace SoftwarePioniere.Messaging
{
    public abstract class MessageBase : IMessage
    {
        protected MessageBase(Guid id, DateTime timeStampUtc, string userId)
        {
            Id = id;
            TimeStampUtc = timeStampUtc;
            UserId = userId;
        }

        /// <summary>
        /// Eindeutige Id der Message
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Zeitstempel als UTC
        /// </summary>
        public DateTime TimeStampUtc { get; } 

        /// <summary>
        /// Benutzername
        /// </summary>
        public string UserId { get; }
    }
}