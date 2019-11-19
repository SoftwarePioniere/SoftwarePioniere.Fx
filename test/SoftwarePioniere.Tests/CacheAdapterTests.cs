using System;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.Caching;
using Foundatio.Lock;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Caching;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.Tests
{
    public class CacheAdapterTests : SoftwarePioniere.DomainModel.TestBase
    {
        public CacheAdapterTests(ITestOutputHelper output) : base(output)
        {

            ServiceCollection
                .AddInMemoryCacheClient()
                .AddInMemoryMessageBus()
                .AddInMemoryEntityStore()
                .AddSingleton<ILockProvider, CacheLockProvider>()
                .AddSingleton<ICacheAdapter, CacheAdapter>();

        }

        [Fact]
        public async Task CanCacheLoad()
        {
            var adapter = GetService<ICacheAdapter>();

            var client = GetService<ICacheClient>();

            var cacheKey = Guid.NewGuid().ToString();

            var item = await adapter.CacheLoad(() => Task.FromResult("hallo"), cacheKey);

            item.Should().Be("hallo");

            var containsCacheKey = await client.ExistsAsync(cacheKey);
            containsCacheKey.Should().BeTrue();


        }


        [Fact]
        public async Task CanCacheLoadItems()
        {
            var adapter = GetService<ICacheAdapter>();

            var client = GetService<ICacheClient>();

            var cacheKey = Guid.NewGuid().ToString();

            var items = await adapter.CacheLoadItems(() =>
            {
                var ret = new[]
                {
                    "hallo1",
                    "hallo2",
                    "hallo3"
                };
                return Task.FromResult(ret);
            }, cacheKey);

            items.Should().Contain("hallo1");
            items.Should().Contain("hallo2");
            items.Should().Contain("hallo3");

            var containsCacheKey = await client.ExistsAsync(cacheKey);
            containsCacheKey.Should().BeTrue();
        }
    }
}
