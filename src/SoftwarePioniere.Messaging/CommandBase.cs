using System;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Basis für ein Command
    /// </summary>
    public abstract class CommandBase : MessageBase, ICommand
    {

        protected CommandBase(Guid id, DateTime timeStampUtc, string userId, int originalVersion, string objectType, string objectId) : base(id, timeStampUtc, userId)
        {
            OriginalVersion = originalVersion;
            ObjectType = objectType;
            ObjectId = objectId;
        }

        public int OriginalVersion { get; }

        public string ObjectType { get; }

        public string ObjectId { get; }

    }

}
