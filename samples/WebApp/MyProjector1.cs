using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;
using SoftwarePioniere.ReadModel;

using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace WebApp
{
    public class MyProjector1 : ReadModelProjectorBase5<FakeEntity>
        , IHandleMessage<FakeEvent>
        , IHandleMessage<FakeEvent2>
    {
        public MyProjector1(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services)
        {
            StreamName = "$ce-SoftwarePionierTests_Fake";
        }

        public async Task HandleAsync(FakeEvent message)
        {
            await LoadAndSaveEveryTimeAsync(message,
                entity => { entity.StringValue = message.Text; });

            var cacheKey = CacheKeys.Create<FakeEntity>("set");
            var entityId = message.AggregateId.CalculateEntityId<FakeEntity>();
            await Services.Cache.SetItemsEnsureAsync(cacheKey, entityId);
        }

        public async Task HandleAsync(FakeEvent2 message)
        {
            await LoadAndSaveEveryTimeAsync(message,
                entity => { entity.StringValue = $"{message.Text}-222"; });

            var cacheKey = CacheKeys.Create<FakeEntity>("set");
            var entityId = message.AggregateId.CalculateEntityId<FakeEntity>();
            await Services.Cache.SetItemsEnsureAsync(cacheKey, entityId);
        }

        public override async Task ProcessEventAsync(IDomainEvent domainEvent)
        {
            await HandleIfAsync<FakeEvent>(HandleAsync, domainEvent);
            await HandleIfAsync<FakeEvent2>(HandleAsync, domainEvent);

            await Services.Cache.CacheClient.RemoveAsync(CacheKeys.Create<FakeEntity>("liste"));
            await Services.Cache.CacheClient.RemoveAsync(CacheKeys.Create<FakeEntity>("liste2"));


        }

        protected override object CreateIdentifierItem(FakeEntity entity)
        {
            return new IdentifierItem
            {
                EntityId = entity.EntityId
            };
        }

        //protected override string CreateLockId(IMessage message)
        //{
        //    if (message is FakeEvent xmsg)
        //    {
        //        return xmsg.AggregateId;
        //    }

        //    if (message is FakeEvent2 xmsg2)
        //    {
        //        return xmsg2.AggregateId;
        //    }

        //    return null;
        //}

        protected override async Task<EntityDescriptor<FakeEntity>> LoadItemAsync(IMessage message)
        {
            var item = new EntityDescriptor<FakeEntity>
            {
                Entity = null
            };

            if (message is FakeEvent xmsg)
            {
                item = await LoadAsync(xmsg.AggregateId);

                //if (xmsg.AggregateId == "7a66ce62-b2ae-4554-b5a2-6b6bf8697f06")
                //    throw new InvalidOperationException();
            }
            else if (message is FakeEvent2 xmsg2)
            {
                item = await LoadAsync(xmsg2.AggregateId);
            }

            return item;
        }

        public class IdentifierItem
        {
            [J("e_id")]
            public string EntityId { get; set; }
        }
    }
}