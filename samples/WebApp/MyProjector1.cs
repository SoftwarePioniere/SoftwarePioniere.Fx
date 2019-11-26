using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;
using SoftwarePioniere.ReadModel;

namespace WebApp
{
    public class MyProjector1 : ReadModelProjectorBase3<FakeEntity>
        , IHandleMessage<FakeEvent>
        , IHandleMessage<FakeEvent2>
    {
        public MyProjector1(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services)
        {
            StreamName = "$ce-SoftwarePionierTests_Fake";
        }

        public Task HandleAsync(FakeEvent message)
        {
            return LoadAndSaveEveryTimeAsync(message,
                entity => { entity.StringValue = message.Text; });
        }

        public Task HandleAsync(FakeEvent2 message)
        {
            return LoadAndSaveEveryTimeAsync(message,
                entity => { entity.StringValue = $"{message.Text}-222"; });
        }
        
        public override async Task ProcessEventAsync(IDomainEvent domainEvent)
        {
            await HandleIfAsync<FakeEvent>(HandleAsync, domainEvent);
            await HandleIfAsync<FakeEvent2>(HandleAsync, domainEvent);
        }

        protected override object CreateIdentifierItem(FakeEntity entity)
        {
            return new IdentifierItem
            {
                EntityId = entity.EntityId
            };
        }

        protected override string CreateLockId(IMessage message)
        {
            if (message is FakeEvent xmsg)
            {
                return xmsg.AggregateId;
            }

            if (message is FakeEvent2 xmsg2)
            {
                return xmsg2.AggregateId;
            }

            return null;
        }

        protected override async Task<EntityDescriptor<FakeEntity>> LoadItemAsync(IMessage message)
        {
            var item = new EntityDescriptor<FakeEntity>
            {
                Entity = null
            };

            if (message is FakeEvent xmsg)
            {
                item = await LoadAsync(xmsg.AggregateId);
            }
            else if (message is FakeEvent2 xmsg2)
            {
                item = await LoadAsync(xmsg2.AggregateId);
            }

            return item;
        }

        public class IdentifierItem
        {
            [JsonProperty("e_id")]
            public string EntityId { get; set; }
        }
    }
}