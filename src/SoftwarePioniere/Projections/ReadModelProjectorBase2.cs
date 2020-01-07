using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.ReadModel;


// ReSharper disable once CheckNamespace
namespace SoftwarePioniere.Projections
{
    public abstract class ReadModelProjectorBase2<T> : ReadModelProjectorBase<T> where T : Entity
    {
        protected ReadModelProjectorBase2(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services.Bus)
        {
            //TelemetryAdapter = services.TelemetryAdapter;
            Cache = services.Cache;
        }

        protected ICacheAdapter Cache { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        protected CancellationToken CancellationToken { get; private set; }

        protected async Task HandleIfAsync<TEvent>(Func<TEvent, Task> handler, IDomainEvent domainEvent)
        {
            if (domainEvent is TEvent message)
            {
                var eventType = typeof(TEvent).FullName;
                var eventId = domainEvent.Id.ToString();

                var state = new Dictionary<string, object>
                {
                    {"EventType", eventType},
                    {"EventId", eventId},
                    {"ProjectorType", GetType().FullName},
                    {"StreamName", StreamName}
                };

                using (Logger.BeginScope(state))
                {
                    Logger.LogDebug($"HANDLE PROJECTOR EVENT {StreamName}/{domainEvent.GetType().Name}");
                    await handler(message);
                }
            }
        }

        public override void Initialize(CancellationToken cancellationToken = new CancellationToken())
        {
            CancellationToken = cancellationToken;
        }

        protected bool LogError(Exception ex)
        {
            Logger.LogError(ex, ex.GetBaseException().Message);
            return true;
        }
    }
}