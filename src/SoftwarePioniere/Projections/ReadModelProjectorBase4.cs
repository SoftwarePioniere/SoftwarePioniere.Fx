using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;
using SoftwarePioniere.ReadModel;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Fliegel365
{
    public abstract class ReadModelProjectorBase4<T> : ReadModelProjectorBase<T> where T : Entity
    {
        protected ITelemetryAdapter0 TelemetryAdapter { get; private set; }

        protected CancellationToken CancellationToken { get; private set; }

        protected ICacheAdapter Cache { get; private set; }

        protected ReadModelProjectorBase4(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services.Bus)
        {
            TelemetryAdapter = services.TelemetryAdapter;
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

        protected Task HandleIfAsync<TEvent>(Func<
                TEvent
                , Task> handler
            , IDomainEvent domainEvent, IDictionary<string, string> parentState)
        {
            if (domainEvent is TEvent message)
            {
                if (parentState == null)
                    parentState = new Dictionary<string, string>();

                var eventType = typeof(TEvent).FullName;
                var eventId = domainEvent.Id.ToString();

                parentState.AddProperty("EventType", eventType)
                    .AddProperty("EventId", eventId)
                    .AddProperty("ProjectorType", GetType().FullName)
                    ;

                if (!string.IsNullOrEmpty(StreamName))
                    parentState.AddProperty("StreamName", StreamName);

                var operationName = $"HANDLE PROJECTOR EVENT {StreamName}/{domainEvent.GetType().Name}";

                return TelemetryAdapter.RunDependencyAsync(operationName,
                    "PROJECTOR",
                    (state) => handler(message),//, state),
                    parentState,
                    Logger);

            }

            return Task.CompletedTask;
        }


        protected virtual async Task<T> DeleteItemIfAsync(IMessage message, Func<T, bool> predicate, IDictionary<string, string> state)
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
                        CreateIdentifierItem, state: state);
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

        protected virtual async Task<T> LoadAndSaveEveryTimeAsync(IMessage message//, IDictionary<string, string> state
          , Action<T> setValues = null)
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

                //if (Context.)
                //    await Cache.AddAsync(item.EntityId, item.Entity);

                await SaveItemAsync(item, message);
                return item.Entity;
            }
        }

        protected virtual async Task<T> LoadAndSaveOnlyExistingAsync(IMessage message//, IDictionary<string, string> state
            , Action<T> setValues = null)
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

        private async Task<T> SaveItemAsync(EntityDescriptor<T> item, IMessage domainEvent)//, IDictionary<string, string> state)
        {

            if (Context.IsLiveProcessing)
            {
                await SaveAsync(item, domainEvent, CreateIdentifierItem(item.Entity));//, state: state);
            }
            else
            {
                await SaveAsync(item, domainEvent, null);//, state: state);
            }

            if (Context.IsReady)
            {
                await Cache.AddAsync(item.EntityId, item.Entity);
            }

            return item.Entity;
        }
    }
}
