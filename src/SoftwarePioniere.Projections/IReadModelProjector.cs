using System.Threading;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public interface IReadModelProjector<T> : IReadModelProjector where T : Entity
    {
        Task<EntityDescriptor<T>> LoadAsync(string itemOnlyId);

        Task SaveAsync(EntityDescriptor<T> ent, IMessage msg, object entityToSerialize);      
    }

    public interface IReadModelProjector : IProjector
    {
        Task CopyEntitiesAsync(IEntityStore source, IEntityStore dest, CancellationToken cancellationToken = default(CancellationToken));

        IProjectionContext Context { get; set; }
    }
}