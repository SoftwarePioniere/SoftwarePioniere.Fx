using System;

namespace SoftwarePioniere.Messaging
{
    public interface IMessage : IUserId
    {

        Guid Id { get; }
        
        DateTime TimeStampUtc { get; }

    }
}