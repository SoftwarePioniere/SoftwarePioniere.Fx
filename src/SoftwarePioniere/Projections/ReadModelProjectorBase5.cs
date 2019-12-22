using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Messaging.Notifications;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
  
    public abstract class ReadModelProjectorBase5<T> : ProjectorBase, IReadModelProjector<T> where T : Entity
    {
        protected readonly ILogger Logger;

        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly IProjectorServices Services;

        // ReSharper disable once MemberCanBePrivate.Global
        protected CancellationToken CancellationToken;

        protected ReadModelProjectorBase5(ILoggerFactory loggerFactory, IProjectorServices services)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Logger = loggerFactory.CreateLogger(GetType());
        }

        public override void Initialize(CancellationToken cancellationToken = new CancellationToken())
        {
            CancellationToken = cancellationToken;
        }

       
        public async Task CopyEntitiesAsync(IEntityStore source, IEntityStore dest, CancellationToken cancellationToken = new CancellationToken())
        {
            Logger.LogDebug("CopyEntitiesAsync Starting");
            var sw = Stopwatch.StartNew();
            {
                Logger.LogTrace("Delete all Items");
                await dest.DeleteAllItemsAsync<T>(cancellationToken);
            }

            var items = await source.LoadItemsAsync<T>(arg => true, cancellationToken);
            var enumerable = items as T[] ?? items.ToArray();
            Logger.LogTrace("Entities Loaded {ItemCount} {EntityType}", enumerable.Length, typeof(T).Name);

            if (enumerable.Any())
            {
                await dest.BulkInsertItemsAsync(enumerable, cancellationToken);
                Logger.LogTrace("Items Inserted");
            }

            Logger.LogDebug("CopyEntitiesAsync Finished in {Elapsed:0.0000} ms ", sw.ElapsedMilliseconds);
        }

        public Task<EntityDescriptor<T>> LoadAsync(string itemOnlyId)
        {
            Logger.LogDebug("LoadAsync {Id}", itemOnlyId);
            return Context.EntityStore.LoadAsync<T>(itemOnlyId, CancellationToken);
        }

        public async Task SaveAsync(EntityDescriptor<T> ent, IMessage msg, object entityToSerialize, Action<NotificationMessage> configureNotification = null)
        {
            Logger.LogDebug("SaveAsync {Id} {IsNew}", ent.EntityId, ent.IsNew);
            ent.Entity.ModifiedOnUtc = msg.TimeStampUtc;
            await Context.EntityStore.SaveAsync(ent, CancellationToken);

            if (Context.IsLiveProcessing)
            {
                Logger.LogDebug("IsLiveProcessing, sending Notification");

                var method = ReadModelUpdatedNotification.MethodInsert;

                if (!ent.IsNew)
                    method = ReadModelUpdatedNotification.MethodUpdate;

                var noti = CreateNotification(ent.Entity, msg, method, entityToSerialize);

                if (noti != null)
                {
                    configureNotification?.Invoke(noti);
                    await Services.Bus.PublishAsync(noti, cancellationToken: CancellationToken); //, state: state);
                }
            }
        }

        protected abstract object CreateIdentifierItem(T entity);

        private NotificationMessage CreateNotification(T entity, IMessage msg, string method, object entityToSerialize)
        {
            var json = entityToSerialize != null ? JsonConvert.SerializeObject(entityToSerialize) : string.Empty;

            return new ReadModelUpdatedNotification
            {
                EntityId = entity.EntityId,
                EntityType = entity.EntityType,
                Method = method,
                Entity = json,
                Reason = msg.GetType().FullName
            }.CreateNotificationMessage(msg);
        }

        private async Task DeleteAsync(T entity, IMessage message)
        {
            await Context.EntityStore.DeleteItemAsync<T>(entity.EntityId, CancellationToken);

            if (Context.IsLiveProcessing)
            {
                var objToSer = CreateIdentifierItem(entity);
                var noti = CreateNotification(entity, message, ReadModelUpdatedNotification.MethodDelete, objToSer);
                if (noti != null) await Services.Bus.PublishAsync(noti, cancellationToken: CancellationToken);
            }
        }

        protected async Task<T> DeleteItemAsync(IMessage message)
        {
            var item = await LoadItemAsync(message);

            if (item.Entity == null) return null;

            if (!item.IsNew) await DeleteAsync(item.Entity, message);
            return item.Entity;
        }

        protected async Task<T> DeleteItemIfAsync(IMessage message, Func<T, bool> predicate)
        {
            var item = await LoadItemAsync(message);

            if (item.Entity == null) return null;

            if (!item.IsNew)
                if (predicate(item.Entity))
                    await DeleteAsync(item.Entity, message);

            return item.Entity;
        }

        protected async Task<bool> HandleIfAsync<TEvent>(Func<TEvent, Task> handler, IDomainEvent domainEvent)
        {
            if (domainEvent is TEvent message)
            {
                var eventType = typeof(TEvent).FullName;
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
                    Logger.LogDebug($"HANDLE PROJECTOR EVENT {StreamName}/{domainEvent.GetType().Name}");
                    await handler(message);
                }

                return true;
            }

            return false;
        }

        protected async Task<T> LoadAndSaveEveryTimeAsync(IMessage message, Action<T> setValues = null)
        {
            var item = await LoadItemAsync(message);
            var entity = item.Entity;

            if (entity == null)
            {
                Logger.LogTrace("No Valid Entity Loaded");
                return null;
            }

            setValues?.Invoke(entity);
            await SaveAsync(item, message, CreateIdentifierItem(entity));
            return item.Entity;
        }

        protected async Task<T> LoadAndSaveOnlyExistingAsync(IMessage message, Action<T> setValues = null)
        {
            var item = await LoadItemAsync(message);
            var entity = item.Entity;

            if (item.IsNew)
            {
                Logger.LogTrace("Skipping new Item");
                return null;
            }

            if (entity == null)
            {
                Logger.LogTrace("No Valid Entity Loaded");
                return null;
            }

            setValues?.Invoke(entity);
            await SaveAsync(item, message, CreateIdentifierItem(entity));
            return item.Entity;
        }

        protected abstract Task<EntityDescriptor<T>> LoadItemAsync(IMessage message);

     
    }

}
