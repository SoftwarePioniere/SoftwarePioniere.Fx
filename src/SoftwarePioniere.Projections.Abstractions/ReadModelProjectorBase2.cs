//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using SoftwarePioniere.Caching;
//using SoftwarePioniere.Messaging;
//using SoftwarePioniere.ReadModel;

//namespace SoftwarePioniere.Projections
//{
//    public abstract class ReadModelProjectorBase2<T> : ReadModelProjectorBase<T> where T : Entity
//    {
//        protected ITelemetryAdapter TelemetryAdapter { get; private set; }

//        protected CancellationToken CancellationToken { get; private set; }

//        protected ICacheAdapter Cache { get; private set; }

//        protected ReadModelProjectorBase2(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services.Bus)
//        {
//            TelemetryAdapter = services.TelemetryAdapter;
//            Cache = services.Cache;
//        }

//        public override void Initialize(CancellationToken cancellationToken = new CancellationToken())
//        {
//            CancellationToken = cancellationToken;
//        }

//        protected bool LogError(Exception ex)
//        {
//            Logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
//            return true;
//        }

//        protected Task HandleIfAsync<TEvent>(Func<
//            TEvent, IDictionary<string, string>
//            , Task> handler
//            , IDomainEvent domainEvent, IDictionary<string, string> parentState)
//        {
//            if (domainEvent is TEvent message)
//            {
//                if (parentState == null)
//                    parentState = new Dictionary<string, string>();

//                var eventType = typeof(TEvent).FullName;
//                var eventId = domainEvent.Id.ToString();

//                parentState.AddProperty("EventType", eventType)
//                        .AddProperty("EventId", eventId)
//                        .AddProperty("ProjectorType", GetType().FullName)
//                    ;

//                if (!string.IsNullOrEmpty(StreamName))
//                    parentState.AddProperty("StreamName", StreamName);

//                var operationName = $"HANDLE PROJECTOR EVENT {StreamName}/{domainEvent.GetType().Name}";

//                return TelemetryAdapter.RunDependencyAsync(operationName,
//                    "PROJECTOR",
//                    (state) => handler(message, state),
//                    parentState,
//                    Logger);

//            }

//            return Task.CompletedTask;
//        }


//    }
//}