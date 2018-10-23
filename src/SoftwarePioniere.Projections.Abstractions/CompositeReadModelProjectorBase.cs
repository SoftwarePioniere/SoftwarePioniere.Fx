using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public abstract class CompositeReadModelProjectorBase : ProjectorBase, IReadModelProjector
    {
        protected readonly ILogger Logger;
        private readonly IList<IReadModelProjector> _childProjectors = new List<IReadModelProjector>();

        protected CompositeReadModelProjectorBase(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger(GetType());
        }

        protected virtual void RegisterChildProjector(IReadModelProjector projector)
        {
            Logger.LogDebug("Registering ChildProjector {ChildProjector}", projector.GetType().Name);
            _childProjectors.Add(projector);
            projector.Context = Context;
        }

        public override async Task HandleAsync(IDomainEvent domainEvent)
        {
            foreach (var projector in _childProjectors)
            {
                Logger.LogDebug("HandleAsync in ChildProjector {ChildProjector}", projector.GetType().Name);
                await projector.HandleAsync(domainEvent);
            }
        }

        public async Task CopyEntitiesAsync(IEntityStore source, IEntityStore dest,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var projector in _childProjectors)
            {
                Logger.LogDebug("CopyEntitiesAsync in ChildProjector {ChildProjector}", projector.GetType().Name);
                await projector.CopyEntitiesAsync(source, dest, cancellationToken);
            }

        }
    }
}