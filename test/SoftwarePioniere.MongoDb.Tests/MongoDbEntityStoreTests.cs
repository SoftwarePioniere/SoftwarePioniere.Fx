using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.ReadModel;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.MongoDb.Tests
{
    [Collection("MongoDbCollection")]
    public class MongoDbEntityStoreTests : EntityStoreTestsBase
    {
        public MongoDbEntityStoreTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection
                .AddOptions()
                .AddMongoDbEntityStoreOptions(options => new TestConfiguration().ConfigurationRoot.Bind("MongoDb", options))
                .AddMongoDbEntityStore();
        }

        [Fact]
        public async Task Test1()
        {
            var mog = GetService<MongoDbEntityStore>();
            var ent = FakeEntity.Create(Guid.NewGuid().ToString());

            await mog.InsertOrUpdateItemAsync(ent);
            await mog.InsertOrUpdateItemAsync(ent);

        }

        [Fact]
        public async Task PerfCheck()
        {
            var prod = GetService<MongoDbConnectionProvider>();
            var store = CreateInstance();

            BulkInsertCount = 10000;
            var chunkId = Guid.NewGuid().ToString();
            var list = FakeEntity.CreateList(BulkInsertCount, chunkId);
            await store.BulkInsertItemsAsync(list);

            _logger.LogInformation("Bulk Insert Finished");
            
            var options = prod.Options;

            async Task F1()
            {
                {
                    //options.FindLimit = 250;
                    options.FindBatchSize = 2000;
                 await RunIt(options, store, chunkId);
                }

                {
                 await RunIt(options, store, chunkId);
                }
            }

            await F1();

            {

                await CreateIndex(Builders<FakeEntity>.IndexKeys
                        .Ascending(x => x.ChunkId)
                    , CancellationToken.None);

                _logger.LogInformation("Indexe erzeugt");
            }

            await F1();
        }

        private async Task CreateIndex<T>(

            //Action<IndexKeysDefinitionBuilder<T>> configureBuilder, 
            IndexKeysDefinition<T> keys,
            CancellationToken cancellationToken = new CancellationToken()) where T : Entity
        {
            //var builder = Builders<T>.IndexKeys;
            //configureBuilder(builder);

            //var keys = builder.Combine();

            var provider = GetService<MongoDbConnectionProvider>();

            await provider.GetCol<T>().Indexes.CreateOneAsync(
                new CreateIndexModel<T>(keys
                ), cancellationToken: cancellationToken);
        }

        private async Task RunIt(MongoDbOptions options, IEntityStore store, string chunkId)
        {

            var sw = Stopwatch.StartNew();
            var all = ( await store.LoadItemsAsync<FakeEntity>(x => x.ChunkId == chunkId)).ToArray();
            sw.Stop();

            all.Length.Should().Be(BulkInsertCount);

            _logger.LogInformation("ReadBatched Duration: {TimeElapsed} - FindLimit:{FindLimit} FindBatchSize:{FindBatchSize}", sw.ElapsedMilliseconds, options.FindLimit, options.FindBatchSize);
        }

        [Fact]
        public override Task CanBulkInsertManyItems()
        {
            BulkInsertCount = 50000;
            return base.CanBulkInsertManyItems();
        }

        [Fact]
        public override void DeleteWithCancelationThrowsError()
        {
            base.DeleteWithCancelationThrowsError();
        }

        [Fact]
        public override void InsertWithCancelationThrowsError()
        {
            base.InsertWithCancelationThrowsError();
        }

        //[Fact]
        //public override void LoadItemsWithPagingAndCancelationThrowsError()
        //{
        //    base.LoadItemsWithPagingAndCancelationThrowsError();
        //}

        [Fact]
        public override void LoadItemsWithCancelationThrowsError()
        {
            base.LoadItemsWithCancelationThrowsError();
        }

        [Fact]
        public override void LoadItemWithCancelationThrowsError()
        {
            base.LoadItemWithCancelationThrowsError();
        }

        [Fact]
        public override void UpdateWithCancelationThrowsError()
        {
            base.UpdateWithCancelationThrowsError();
        }

        [Fact]
        public override Task CanInsertAndDeleteItem()
        {
            return base.CanInsertAndDeleteItem();
        }

        [Fact]
        public override Task CanInsertAndUpdateItem()
        {
            return base.CanInsertAndUpdateItem();
        }

        [Fact]
        public override Task CanInsertItem()
        {
            return base.CanInsertItem();
        }

        [Fact]
        public override void DeleteThrowsErrorWithKeyNullOrEmpty()
        {
            base.DeleteThrowsErrorWithKeyNullOrEmpty();
        }

        [Fact]
        public override void LoadItemThrowsErrorWithKeyNullOrEmpty()
        {
            base.LoadItemThrowsErrorWithKeyNullOrEmpty();
        }

        //[Fact]
        //public override Task LoadItemsWithPagingWorks()
        //{
        //    return base.LoadItemsWithPagingWorks();
        //}

        [Fact]
        public override Task LoadItemsWithWhereWorks()
        {
            return base.LoadItemsWithWhereWorks();
        }

        [Fact]
        public override Task SaveAndLoadItemPropertiesEquals()
        {
            return base.SaveAndLoadItemPropertiesEquals();
        }

        [Fact]
        public override Task SaveAndLoadItemsContainsAll()
        {
            return base.SaveAndLoadItemsContainsAll();
        }

        [Fact]
        public override Task SaveAndUpdateItemPropertiesEquals()
        {
            return base.SaveAndUpdateItemPropertiesEquals();
        }

        [Fact]
        public override void SaveThrowsErrorWithItemNull()
        {
            base.SaveThrowsErrorWithItemNull();
        }

        [Fact]
        public override Task CanInsertManyItems()
        {
            return base.CanInsertManyItems();
        }

        [Fact]
        public override Task InsertExistingWillUpdate()
        {
            return base.InsertExistingWillUpdate();
        }


        [Fact]
        public override Task UpdateNonExistingWillInsert()
        {
            return base.UpdateNonExistingWillInsert();
        }

        [Fact]
        public override Task CanInsertAndDeleteAllItems()
        {
            return base.CanInsertAndDeleteAllItems();
        }

        [Fact]
        public override Task CanInsertAndDeleteItemWithWhere()
        {
            return base.CanInsertAndDeleteItemWithWhere();
        }

        [Fact]
        public override Task CanHandleDictionaries()
        {
            return base.CanHandleDictionaries();
        }
    }
}
