using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Projections
{
    public abstract class ProjectorBase : IProjector
    {
        public abstract void Initialize(CancellationToken cancellationToken = default(CancellationToken));

        public string StreamName { get; protected set; }

        public abstract Task HandleAsync(IDomainEvent domainEvent, IDictionary<string, string> state = null);

        public virtual IProjectionContext Context { get; set; }
    }
}
