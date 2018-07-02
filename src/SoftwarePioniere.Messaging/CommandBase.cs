using System;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Basis für ein Command
    /// </summary>
    public abstract class CommandBase : MessageBase, ICommand
    {
        //protected Command(Guid id, DateTime timeStampUtc, string userId) : this(id, timeStampUtc, userId, -1)
        //{

        //}

        protected CommandBase(Guid id, DateTime timeStampUtc, string userId, int originalVersion, string requestId) : base(id, timeStampUtc, userId)
        {
            OriginalVersion = originalVersion;
            RequestId = requestId;
        }


        /// <summary>
        /// Die EventVersion des Original Objekts
        /// </summary>
        public int OriginalVersion { get; }

        /// <summary>
        /// Correlation Id des Requests, z.B. vom Webserver
        /// </summary>

        public string RequestId { get; }
    }
}
