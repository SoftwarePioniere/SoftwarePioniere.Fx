using System;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Allgemeine Nachricht, die im System versendet und verwendet wird
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Benutzername
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Eindeutige Id der Message
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Zeitstempel als UTC
        /// </summary>
        DateTime TimeStampUtc { get; }

    }
}