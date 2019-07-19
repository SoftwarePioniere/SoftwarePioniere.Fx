using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;

namespace Fliegel365
{
    public abstract class CompositeReadModelProjectorBase2 : CompositeReadModelProjectorBase
    {
        protected readonly ITelemetryAdapter0 TelemetryAdapter;

        protected CompositeReadModelProjectorBase2(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory)
        {
            TelemetryAdapter = services.TelemetryAdapter;
        }

        public override Task ProcessEventAsync(IDomainEvent domainEvent, IDictionary<string, string> parentState = null)
        {
            if (parentState == null)
                parentState = new Dictionary<string, string>();

            var eventType = domainEvent.GetType().FullName;
            var eventId = domainEvent.Id.ToString();

            parentState.AddProperty("EventType", eventType)
                .AddProperty("EventId", eventId)
                .AddProperty("StreamName", StreamName)
                .AddProperty("ProjectorType", GetType().FullName)
                ;

            var operationName = $"HANDLE PROJECTOR EVENT {StreamName}/{domainEvent.GetType().Name}";

            return TelemetryAdapter.RunDependencyAsync(operationName,
                "PROJECTOR",
                (state) => base.ProcessEventAsync(domainEvent, state),
                parentState,
                Logger);

        }

        protected bool LogError(Exception ex)
        {
            Logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
            return true;
        }

    }
}