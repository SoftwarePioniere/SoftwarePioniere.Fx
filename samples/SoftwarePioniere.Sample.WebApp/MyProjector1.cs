using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoftwarePioniere.DomainModel.FakeDomain;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Sample.WebApp
{
    public class MyProjector1 : ReadModelProjectorBase3<FakeEntity>
        , IHandleWithState<FakeEvent>
        , IHandleWithState<FakeEvent2>
    {
        public MyProjector1(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services)
        {
            StreamName = "$ce-SoftwarePionierTests_Fake";
        }

        public Task HandleAsync(FakeEvent message, IDictionary<string, string> state)
        {
            return LoadAndSaveEveryTimeAsync(message,
                state,
                entity => { entity.StringValue = message.Text; });
        }

        public Task HandleAsync(FakeEvent2 message, IDictionary<string, string> state)
        {
            return LoadAndSaveEveryTimeAsync(message,
                state,
                entity => { entity.StringValue = $"{message.Text}-222"; });
        }

        /*public async Task HandleAsync(FakeEvent message, IDictionary<string, string> state)
        {
            var item = await LoadAsync(message.AggregateId);
            item.Entity.StringValue = message.Text;
            await SaveAsync(item, message, item.Entity, state: state);
        }

        public async Task HandleAsync(FakeEvent2 message, IDictionary<string, string> state)
        {
            var item = await LoadAsync(message.AggregateId);
            item.Entity.StringValue = $"{message.Text}-222";
            await SaveAsync(item, message, item.Entity, state: state);
        }*/

        public override async Task ProcessEventAsync(IDomainEvent domainEvent, IDictionary<string, string> state = null)
        {
            await HandleIfAsync<FakeEvent>(HandleAsync, domainEvent, state);
            await HandleIfAsync<FakeEvent2>(HandleAsync, domainEvent, state);
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