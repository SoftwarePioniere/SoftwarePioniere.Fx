using System;
using SoftwarePioniere.DomainModel.Exceptions;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    [AggregateName(Constants.BoundedContextName, "Fake")]
    public class FakeAggregate : AggregateRoot
        , IApplyEvent<FakeEvent>
        , IApplyEvent<FakeEvent2>
    {
        public string Text { get; private set; }

        public void ApplyEvent(FakeEvent @event)
        {
            Text = @event.Text;
        }

        public void ApplyEvent(FakeEvent2 @event)
        {
            Text = @event.Text;
        }

        public void DoFakeEvent(string text)
        {
            if (string.IsNullOrEmpty(Id))
                throw new DomainLogicException(this, "please set id first");

            var @event = new FakeEvent(Guid.NewGuid(), DateTime.UtcNow, "userId", Id, text);
            ApplyChange(@event);
        }

        public void DoFakeEvent2(string text)
        {
            if (string.IsNullOrEmpty(Id))
                throw new DomainLogicException(this, "please set id first");

            var @event = new FakeEvent2(Guid.NewGuid(), DateTime.UtcNow, "userId", Id, text);
            ApplyChange(@event);
        }

        public void DoFakeEvent3()
        {
            var @event = new FakeEvent3(Guid.NewGuid(), DateTime.UtcNow, "userId", Id);
            ApplyChange(@event);
        }

        public class AggregateId : BaseAggregateId
        {
            public AggregateId(string id) : base(id)
            {
            }
        }

        public static class Factory
        {
            public static FakeAggregate Create(string fakeId)
            {
                var agg = new FakeAggregate();
                agg.SetId(new AggregateId(fakeId).Id);
                return agg;
            }


            public static FakeAggregate Create()
            {
                return Create(Guid.NewGuid().ToString());
            }

        }


    }
}