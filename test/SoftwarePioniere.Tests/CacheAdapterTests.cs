using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.Caching;
using Foundatio.Lock;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Caching;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.ReadModel;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.Tests
{
    public class CacheAdapterTests : Domain.TestBase
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

            async Task<string[]> LoadItems()
            {
                return await adapter.CacheLoadItems(() =>
                {
                    var ret = new[]
                    {
                        "hallo1",
                        "hallo2",
                        "hallo3"
                    };
                    return Task.FromResult(ret.AsEnumerable());
                }, cacheKey);
            }

            var items = await LoadItems();

            items.Should().Contain("hallo1");
            items.Should().Contain("hallo2");
            items.Should().Contain("hallo3");

            var containsCacheKey = await client.ExistsAsync(cacheKey);
            containsCacheKey.Should().BeTrue();


            items = await LoadItems();

            items.Should().Contain("hallo1");
            items.Should().Contain("hallo2");
            items.Should().Contain("hallo3");
        }



        [Fact]
        public async Task CanCacheLoadSetItems()
        {
            // add items to store
            var store = GetService<IEntityStore>();
            var chunkid = Guid.NewGuid().ToString();
            var list1 = FakeEntity.CreateList(15, chunkid).ToArray();
            await store.BulkInsertItemsAsync(list1);

            var adapter = GetService<ICacheAdapter>();
            var client = GetService<ICacheClient>();


            var setKey = Guid.NewGuid().ToString();


            var items1 = await adapter.LoadSetItems<FakeEntity>(setKey,
                entity => entity.ChunkId == chunkid);

            items1.Count.Should().Be(list1.Length);

            var containsCacheKey = await client.ExistsAsync(setKey);
            containsCacheKey.Should().BeTrue();

            async Task RunLogic()
            {
                var item2 = FakeEntity.CreateList(1, chunkid).First();
                await store.InsertItemAsync(item2);

                await adapter.SetItemsEnsureAsync(setKey, item2.EntityId);

                var items2 = await adapter.LoadSetItems<FakeEntity>(setKey,
                    entity => entity.ChunkId == chunkid);

                items2.Should().Contain(x => x.EntityId == item2.EntityId);
            }

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(RunLogic());
            }

            await Task.WhenAll(tasks);
        }
    }
}
