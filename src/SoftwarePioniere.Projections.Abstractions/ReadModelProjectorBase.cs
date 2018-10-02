﻿using System;
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
            ent.Entity.ModifiedOnUtc = msg.TimeStampUtc;
            await Context.EntityStore.SaveAsync(ent);

            if (Context.IsLiveProcessing)
            {
                Logger.LogDebug("IsLiveProcessing, sending Notification");
                await Bus.PublishAsync(CreateNotification(ent, msg, entityToSerialize));
            }
        }

        public virtual async Task DeleteAsync(string itemOnlyId, IMessage message, Func<T, object> objectToSeriaizer = null)
        {
            var item = await LoadAsync(itemOnlyId);

            if (!item.IsNew)
            {
                await Context.EntityStore.DeleteItemAsync<T>(item.EntityId);

                if (Context.IsLiveProcessing)
                {
                    object objToSer = null;

                    if (objectToSeriaizer != null)
                    {
                        objToSer = objectToSeriaizer(item.Entity);
                    }

                    var noti = CreateNotification(item.Entity, message, ReadModelUpdatedNotification.MethodDelete, objToSer);
                    await Bus.PublishAsync(noti);
                }
            }
        }


        public virtual async Task CopyEntitiesAsync(IEntityStore source, IEntityStore dest, CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.LogInformation("Copy Entities");

            {
                var destItems =  await dest.LoadItemsAsync<T>(arg => true, cancellationToken);
                if (destItems.Length > 0)
                {
                    Logger.LogDebug("Entities in Destination Loaded {ItemCount} {EntityType} - Must Delete", destItems.Length, typeof(T).Name);

                    foreach (var destItem in destItems)
                    {
                        await dest.DeleteItemAsync<T>(destItem.EntityId, cancellationToken);
                    }
                }
            }

            var items = await source.LoadItemsAsync<T>(arg => true, cancellationToken);
            Logger.LogDebug("Entities Loaded {ItemCount} {EntityType}", items.Length, typeof(T).Name);

            if (items.Length > 0)
            {
                await dest.BulkInsertItemsAsync(items, cancellationToken);
                Logger.LogDebug("Items Inserted");
            }
            //foreach (var item in items)
            //{
            //    cancellationToken.ThrowIfCancellationRequested();
            //    await dest.InsertItemAsync(item, cancellationToken);
            //}
           
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
