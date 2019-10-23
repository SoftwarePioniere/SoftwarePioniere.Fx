using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Caching;
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

        public Task<FakeEntity[]> GetListeAsync(ClaimsPrincipal user)
        {
            var items = _cache.CacheLoad(() => _entityStore.LoadItemsAsync<FakeEntity>(),
                CacheKeys.Create<FakeEntity>("liste"),
                logger: _logger
            );

            return items;
        }
    }
}