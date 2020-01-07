using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Caching;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.ReadModel;

namespace WebApp.Controller
{
    public class MyQueryService
    {
        private readonly ICacheAdapter _cache;
        private readonly IEntityStore _entityStore;
        private readonly ILogger _logger;

        public MyQueryService(ILoggerFactory loggerFactory, ICacheAdapter cache, IEntityStore entityStore)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _entityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public Task<IEnumerable<FakeEntity>> GetListeAsync(ClaimsPrincipal user)
        {
            _logger.LogInformation("GetListeAsync");

            return _cache.CacheLoad(() => _entityStore.LoadItemsAsync<FakeEntity>(),
                 CacheKeys.Create<FakeEntity>("liste"));

        }


        public async Task<IEnumerable<FakeEntity>> GetListe2Async(ClaimsPrincipal user)
        {
            _logger.LogInformation("GetListe2Async");

            return await _cache.CacheLoadItems(() => _entityStore.LoadItemsAsync<FakeEntity>(),
                CacheKeys.Create<FakeEntity>("liste2"));
        }

        public async Task<IEnumerable<FakeEntity>> GetListe3Async(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetListe3Async");
            
            var items = await _cache.LoadSetItems<FakeEntity>(
                CacheKeys.Create<FakeEntity>("set")
                , x => true, cancellationToken: cancellationToken);

            return items;

        }
    }
}