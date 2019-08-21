using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Lock;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public abstract class ReadModelProjectorBase3<T> : ReadModelProjectorBase2<T> where T : Entity
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected ILockProvider LockProvider { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        protected string EntityTypeKey { get; private set; }

        protected ReadModelProjectorBase3(ILoggerFactory loggerFactory, IProjectorServices services) : base(loggerFactory, services)
        {
            LockProvider = new CacheLockProvider(
                new InMemoryCacheClient(builder => builder.LoggerFactory(loggerFactory))
                , new InMemoryMessageBus(builder => builder.LoggerFactory(loggerFactory)), loggerFactory);
            //LockProvider = services.LockProvider;

            var ent = Activator.CreateInstance<T>();
            EntityTypeKey = ent.EntityType;

        }

        private string GetLockId(IMessage evnt)
        {
            var lockId = $"{EntityTypeKey}-{CreateLockId(evnt)}";

            if (string.IsNullOrEmpty(lockId))
                lockId = Guid.NewGuid().ToString();

            return lockId;
        }

        private string GetCacheLockId()
        {
            var lockId = $"CACHE-{EntityTypeKey}";
            return lockId;
        }

        protected async Task DeleteItemIfAsync(IMessage message, Func<T, bool> predicate, IDictionary<string, string> state)
        {
            async Task DoAsync()
            {
                var lockId = GetLockId(message);
                await LockProvider.TryUsingAsync(lockId,
                    async token =>
                    {

                        var item = await LoadItemAsync(message);

                        if (item.Entity == null)
                        {
                            return;
                        }

                        if (!item.IsNew)
                        {
                            if (predicate(item.Entity))
                            {
                                await DeleteAsync(item.Id,
                                    message,
                                    CreateIdentifierItem, state: state);


                            }
                        }
                    }, cancellationToken: CancellationToken.None);
            }

            await LockProvider.TryUsingAsync(GetCacheLockId(), async () => await DoAsync(), timeUntilExpires: null, cancellationToken: CancellationToken.None);
        }

        protected async Task DeleteItemAsync(IMessage message)
        {
            async Task DoAsync()
            {
                var item = await LoadItemAsync(message);

                if (item.Entity == null)
                {
                    return;
                }

                if (!item.IsNew)
                {
                    await base.DeleteAsync(item.Id,
                        message,
                        CreateIdentifierItem);
                        //,state: state);
                }
            }
            await LockProvider.TryUsingAsync(GetCacheLockId(), async () => await DoAsync(), timeUntilExpires: null, cancellationToken: CancellationToken.None);
        }

        protected abstract string CreateLockId(IMessage message);

        protected abstract object CreateIdentifierItem(T entity);

        protected abstract Task<EntityDescriptor<T>> LoadItemAsync(IMessage message);

        protected async Task LoadAndSaveEveryTimeAsync(IMessage message//, IDictionary<string, string> state
            , Action<T> setValues = null)
        {
            var lockId = GetLockId(message);

            await LockProvider.TryUsingAsync(lockId,
                async token =>
                {

                    var item = await LoadItemAsync(message);
                    var entity = item.Entity;

                    if (entity == null)
                    {
                        Logger.LogTrace("No Valid Entity Loaded");
                    }
                    else
                    {
                        setValues?.Invoke(entity);

                      //      await SaveItemAsync(item, message, state);
                      await SaveItemAsync(item, message);
                    }
                }, cancellationToken: CancellationToken.None);
        }

        protected async Task LoadAndSaveOnlyExistingAsync(IMessage message//, IDictionary<string, string> state
            , Action<T> setValues = null)
        {
            var lockId = GetLockId(message);

            await LockProvider.TryUsingAsync(lockId,
                async token =>
                {

                    var item = await LoadItemAsync(message);
                    var entity = item.Entity;

                    if (item.IsNew)
                    {
                        Logger.LogTrace("Skipping new Item");
                    }
                    else
                    {
                        if (entity == null)
                        {
                            Logger.LogTrace("No Valid Entity Loaded");
                        }
                        else
                        {

                            setValues?.Invoke(entity);

                          //  await SaveItemAsync(item, message, state);
                          await SaveItemAsync(item, message);
                        }
                    }
                }, cancellationToken: CancellationToken.None);
        }

        private async Task SaveItemAsync(EntityDescriptor<T> item, IMessage domainEvent)//, IDictionary<string, string> state)
        {
            async Task DoAsync()
            {

                if (Context.IsLiveProcessing)
                {
                    await SaveAsync(item, domainEvent, CreateIdentifierItem(item.Entity));//, state: state);
                }
                else
                {
                    await SaveAsync(item, domainEvent, null);//, state: state);
                }

                await Cache.RemoveByPrefixAsync(EntityTypeKey);
            }

            await LockProvider.TryUsingAsync(GetCacheLockId(), async () => await DoAsync(), timeUntilExpires: null, cancellationToken: CancellationToken.None);
        }

    }
}