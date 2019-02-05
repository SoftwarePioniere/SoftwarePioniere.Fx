using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Projections
{
    public interface IProjector
    {

        void Initialize(CancellationToken cancellationToken = default(CancellationToken));

        string StreamName { get; }

        Task HandleAsync(IDomainEvent domainEvent, IDictionary<string, string> state = null);
    }
}
