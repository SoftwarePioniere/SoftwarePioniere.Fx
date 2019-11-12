using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Caching;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.ReadModel;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SoftwarePioniere.Projections
{
    public abstract class ReadModelProjectorBase4<T> : ReadModelProjectorBase<T> where T : Entity
    {

        protected CancellationToken CancellationToken { get; private set; }

        protected ICacheAdapter Cache { get; private set; }

        protected ReadModelProjectorBase4(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services.Bus)
        {
            Cache = services.Cache;
        }

        protected bool LogError(Exception ex)
        {
            Logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
            return true;
        }

        public override void Initialize(CancellationToken cancellationToken = new CancellationToken())
        {
            CancellationToken = cancellationToken;
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


        protected virtual async Task<T> DeleteItemIfAsync(IMessage message, Func<T, bool> predicate)
        {

            var item = await LoadItemAsync(message);

            if (item.Entity == null)
            {
                return null;
            }

            if (!item.IsNew)
            {
                if (predicate(item.Entity))
                {

                    if (Context.IsReady)
                        await Cache.CacheClient.RemoveAsync(item.EntityId);

                    await DeleteAsync(item.Id,
                        message,
                        CreateIdentifierItem);
                }
            }

            return item.Entity;
        }

        protected virtual async Task<T> DeleteItemAsync(IMessage message)
        {
            var item = await LoadItemAsync(message);

            if (item.Entity == null)
            {
                return null;
            }

            if (!item.IsNew)
            {
                if (Context.IsReady)
                    await Cache.CacheClient.RemoveAsync(item.EntityId);

                await base.DeleteAsync(item.Id,
                    message,
                    CreateIdentifierItem);
            }
            return item.Entity;
        }

        protected abstract string CreateLockId(IMessage message);

        protected abstract object CreateIdentifierItem(T entity);

        protected abstract Task<EntityDescriptor<T>> LoadItemAsync(IMessage message);

        protected virtual async Task<T> LoadAndSaveEveryTimeAsync(IMessage message, Action<T> setValues = null)
        {

            var item = await LoadItemAsync(message);
            var entity = item.Entity;

            if (entity == null)
            {
                Logger.LogTrace("No Valid Entity Loaded");
                return null;
            }
            else
            {
                setValues?.Invoke(entity);

                if (Context.IsReady)
                    await Cache.AddAsync(item.EntityId, item.Entity);

                await SaveItemAsync(item, message);
                return item.Entity;
            }
        }

        protected virtual async Task<T> LoadAndSaveOnlyExistingAsync(IMessage message, Action<T> setValues = null)
        {

            var item = await LoadItemAsync(message);
            var entity = item.Entity;

            if (item.IsNew)
            {
                Logger.LogTrace("Skipping new Item");
                return null;
            }
            else
            {
                if (entity == null)
                {
                    Logger.LogTrace("No Valid Entity Loaded");
                    return null;
                }
                else
                {

                    setValues?.Invoke(entity);

                    await SaveItemAsync(item, message);
                    return item.Entity;
                }
            }

        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task<T> SaveItemAsync(EntityDescriptor<T> item, IMessage domainEvent){

            if (Context.IsLiveProcessing)
            {
                await SaveAsync(item, domainEvent, CreateIdentifierItem(item.Entity));
            }
            else
            {
                await SaveAsync(item, domainEvent, null);
            }

            return item.Entity;
        }
    }
}
