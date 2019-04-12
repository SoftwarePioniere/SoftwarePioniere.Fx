using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public class MyEntityProjector1 : ReadModelProjectorBase<MyEntity1>
    {
        public MyEntityProjector1(ILoggerFactory loggerFactory, IMessageBusAdapter bus) : base(loggerFactory, bus)
        {
            StreamName = "$ce-SoftwarePionierTests_Fake";
        }

        public override void Initialize(CancellationToken cancellationToken = default(CancellationToken))
        {

        }

        public override async Task ProcessEventAsync(IDomainEvent domainEvent, IDictionary<string, string> state = null)
        {
            if (domainEvent is FakeEvent fe)
            {
                var evnt = fe;

                Logger.LogDebug("HandleAsync {MessageType} {@Message}", evnt.GetType().Name, evnt);
                var item = await LoadAsync(evnt.AggregateId);
                var entity = item.Entity;

                // await Task.Delay(200);

                entity.StringValue = evnt.Text;

                await SaveAsync(item, domainEvent, new { id = entity.EntityId });
            }
        }
    }


}
