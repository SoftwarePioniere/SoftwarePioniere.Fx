using System;
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
                await dest.DeleteAllItemsAsync<T>(cancellationToken).ConfigureAwait(false);
            }

            var items = await source.LoadItemsAsync<T>(arg => true, cancellationToken).ConfigureAwait(false);
            var enumerable = items as T[] ?? items.ToArray();
            Logger.LogTrace("Entities Loaded {ItemCount} {EntityType}", enumerable.Length, typeof(T).Name);

            if (enumerable.Any())
            {
                await dest.BulkInsertItemsAsync(enumerable, cancellationToken).ConfigureAwait(false);
                Logger.LogTrace("Items Inserted");
            }

            Logger.LogDebug("CopyEntitiesAsync Finished in {Elapsed} ms ", sw.ElapsedMilliseconds);
        }

        public Task<EntityDescriptor<T>> LoadAsync(string itemOnlyId)
        {
            Logger.LogDebug("LoadAsync {Id}", itemOnlyId);
            return Context.EntityStore.LoadAsync<T>(itemOnlyId, CancellationToken);
        }

        public async Task SaveAsync(EntityDescriptor<T> ent, IMessage msg, object entityToSerialize, Action<NotificationMessage> configureNotification = null)
        {
            var sw = Stopwatch.StartNew();
            Logger.LogDebug("SaveAsync started");

            Logger.LogDebug("SaveAsync {Id} {IsNew}", ent.EntityId, ent.IsNew);
            ent.Entity.ModifiedOnUtc = msg.TimeStampUtc;
            await Context.EntityStore.SaveAsync(ent, CancellationToken).ConfigureAwait(false);

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
                    await Services.Bus.PublishAsync(noti, cancellationToken: CancellationToken).ConfigureAwait(false); //, state: state);
                }
            }

            sw.Stop();
            Logger.LogDebug("SaveAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);
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
            var sw = Stopwatch.StartNew();
            Logger.LogDebug("DeleteAsync started");

            await Context.EntityStore.DeleteItemAsync<T>(entity.EntityId, CancellationToken).ConfigureAwait(false);

            if (Context.IsLiveProcessing)
            {
                var objToSer = CreateIdentifierItem(entity);
                var noti = CreateNotification(entity, message, ReadModelUpdatedNotification.MethodDelete, objToSer);
                if (noti != null) await Services.Bus.PublishAsync(noti, cancellationToken: CancellationToken).ConfigureAwait(false);
            }

            sw.Stop();
            Logger.LogDebug("DeleteAsync finished in {Elapsed} ms", sw.ElapsedMilliseconds);
        }

        protected async Task<T> DeleteItemAsync(IMessage message)
        {
            var item = await LoadItemAsync(message).ConfigureAwait(false);

            if (item.Entity == null) return null;

            if (!item.IsNew) await DeleteAsync(item.Entity, message).ConfigureAwait(false);
            return item.Entity;
        }

        protected async Task<T> DeleteItemIfAsync(IMessage message, Func<T, bool> predicate)
        {
            var item = await LoadItemAsync(message).ConfigureAwait(false);

            if (item.Entity == null) return null;

            if (!item.IsNew)
                if (predicate(item.Entity))
                    await DeleteAsync(item.Entity, message).ConfigureAwait(false);

            return item.Entity;
        }

        protected async Task<bool> HandleIfAsync<TEvent>(Func<TEvent, Task> handler, IDomainEvent domainEvent)
        {
            if (domainEvent is TEvent message)
            {
                //var eventType = typeof(TEvent).FullName;
                //var eventId = domainEvent.Id.ToString();

                //var state = new Dictionary<string, object>
                //{
                //    {"EventType", eventType},
                //    {"EventId", eventId},
                //    {"ProjectorType", GetType().FullName},
                //    {"StreamName", StreamName}
                //};

                //using (Logger.BeginScope(state))
                //{
                Logger.LogDebug($"HANDLE PROJECTOR EVENT {StreamName}/{domainEvent.GetType().Name}");
                await handler(message).ConfigureAwait(false);
                //}

                return true;
            }

            return false;
        }

        protected async Task<T> LoadAndSaveEveryTimeAsync(IMessage message, Action<T> setValues = null)
        {
            var item = await LoadItemAsync(message).ConfigureAwait(false);
            var entity = item.Entity;

            if (entity == null)
            {
                Logger.LogTrace("No Valid Entity Loaded");
                return null;
            }

            setValues?.Invoke(entity);
            await SaveAsync(item, message, CreateIdentifierItem(entity)).ConfigureAwait(false);
            return item.Entity;
        }

        protected async Task<T> LoadAndSaveOnlyExistingAsync(IMessage message, Action<T> setValues = null)
        {
            var item = await LoadItemAsync(message).ConfigureAwait(false);
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
            await SaveAsync(item, message, CreateIdentifierItem(entity)).ConfigureAwait(false);
            return item.Entity;
        }

        protected abstract Task<EntityDescriptor<T>> LoadItemAsync(IMessage message);


    }

}
