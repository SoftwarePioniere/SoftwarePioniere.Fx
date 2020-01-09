using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public abstract class CompositeReadModelProjectorBase : IReadModelProjector
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly ILogger Logger;
        private readonly IList<IReadModelProjector> _childProjectors = new List<IReadModelProjector>();
        private IProjectionContext _context;

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

        public abstract void Initialize(CancellationToken cancellationToken = default(CancellationToken));

        public string StreamName { get; protected set; }

        public virtual async Task ProcessEventAsync(IDomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType().FullName;
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
                var sw = Stopwatch.StartNew();
                Logger.LogDebug("ProcessEventAsync started");

                try
                {
                    var tasks = _childProjectors.Select(x => x.ProcessEventAsync(domainEvent));
                    await Task.WhenAll(tasks);
                }
                catch (Exception e) when (LogError(e))
                {
                }

                sw.Stop();
                Logger.LogDebug("ProcessEventAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);

            }
        }

        public virtual async Task CopyEntitiesAsync(IEntityStore source, IEntityStore dest,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            Logger.LogDebug("CopyEntitiesAsync started");

            var tasks = _childProjectors.Select(x => x.CopyEntitiesAsync(source, dest, cancellationToken));
            await Task.WhenAll(tasks);
            
            sw.Stop();
            Logger.LogDebug("CopyEntitiesAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected bool LogError(Exception ex)
        {
            Logger.LogError(ex, ex.GetBaseException().Message);
            return true;
        }

        public IProjectionContext Context
        {
            get => _context;
            set
            {
                _context = value;
                foreach (var projector in _childProjectors)
                {
                    projector.Context = _context;
                }
            }
        }
    }
}