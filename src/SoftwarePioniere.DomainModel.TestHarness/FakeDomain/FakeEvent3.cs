using System;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public class FakeEvent3 : DomainEventBase
    {
      
        public FakeEvent3(Guid id, DateTime timeStampUtc, string userId, string aggregateId) : base(id, timeStampUtc, userId, aggregateId)
        {

        }
    
    }
}