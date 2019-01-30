using System;
using System.Threading;
using System.Threading.Tasks;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Messaging.Notifications;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public interface IReadModelProjector<T> : IReadModelProjector where T : Entity
    {
        Task<EntityDescriptor<T>> LoadAsync(string itemOnlyId);

        Task SaveAsync(EntityDescriptor<T> ent, IMessage msg, object entityToSerialize, Action<NotificationMessage> configureNotification = null);      
    }

    public interface IReadModelProjector : IProjector
    {
        Task CopyEntitiesAsync(IEntityStore source, IEntityStore dest, CancellationToken cancellationToken = default(CancellationToken));

        IProjectionContext Context { get; set; }
    }
}