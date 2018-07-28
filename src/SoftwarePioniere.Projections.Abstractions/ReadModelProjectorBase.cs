using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Messaging.Notifications;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{

    public abstract class ReadModelProjectorBase<T> : ProjectorBase, IReadModelProjector<T> where T : Entity
    {
        protected readonly ILogger Logger;

        protected readonly IMessageBus Bus;

        protected ReadModelProjectorBase(ILoggerFactory loggerFactory, IMessageBus bus)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger(GetType());
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public virtual async Task<EntityDescriptor<T>> LoadAsync(string itemOnlyId)
        {
            Logger.LogDebug("LoadAsync {Id}", itemOnlyId);
            return await Context.EntityStore.LoadAsync<T>(itemOnlyId);
        }

        public virtual async Task SaveAsync(EntityDescriptor<T> ent, IMessage msg, object entityToSerialize)
        {
            Logger.LogDebug("SaveAsync {Id} {IsNew}", ent.EntityId, ent.IsNew);
            await Context.EntityStore.SaveAsync(ent);

            if (Context.IsLiveProcessing)
            {
                Logger.LogDebug("IsLiveProcessing, sending Notification");
                await Bus.PublishAsync(CreateNotification(ent, msg, entityToSerialize));
            }
        }

        public virtual async Task CopyEntitiesAsync(IEntityStore source, IEntityStore dest, CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.LogInformation("Copy Entities");

            var items = await source.LoadItemsAsync<T>(arg => true, cancellationToken);
            Logger.LogDebug("Entities Loaded {ItemCount} {EntityType}", items.Length, typeof(T).Name);


            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await dest.InsertItemAsync(item, cancellationToken);
            }
            Logger.LogDebug("Items Inserted");

            cancellationToken.ThrowIfCancellationRequested();

            var status = await source.LoadItemAsync<ProjectionStatus>(Context.ProjectorId.CalculateEntityId<ProjectionStatus>(), cancellationToken);
            await dest.InsertItemAsync(status, cancellationToken);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual NotificationMessage CreateNotification(Entity entity, IMessage msg, string method, object entityToSerialize)
        {
            var json = entityToSerialize != null ? JsonConvert.SerializeObject(entityToSerialize) : string.Empty;

            return new ReadModelUpdatedNotification
            {
                EntityId = entity.EntityId,
                EntityType = entity.EntityType,
                Method = method,
                Entity = json
            }.CreateNotificationMessage(msg);
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual NotificationMessage CreateNotification(EntityDescriptor<T> descriptor, IMessage msg, object entityToSerialize)
        {
            var method = ReadModelUpdatedNotification.MethodInsert;

            if (!descriptor.IsNew)
                method = ReadModelUpdatedNotification.MethodUpdate;

            return CreateNotification(descriptor.Entity, msg, method, entityToSerialize);
        }
      
    }
}
