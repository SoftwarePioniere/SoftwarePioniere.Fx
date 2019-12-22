using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.Caching;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.FakeDomain;
using Xunit.Abstractions;

namespace SoftwarePioniere.ReadModel
{
    //public interface IEntityStoreTests
    //{
    //    Task CanInsertAndDeleteItem();

    //    Task CanInsertAndUpdateItem();

    //    Task CanInsertItem();

    //    Task CanInsertManyItems();

    //    Task CanBulkInsertManyItems();

    //    void DeleteThrowsErrorWithKeyNullOrEmpty();

    //    void DeleteWithCancelationThrowsError();

    //    void InsertWithCancelationThrowsError();

    //    Task LoadItemsWithPagingWorks();

    //    void LoadItemsWithPagingAndCancelationThrowsError();

    //    //Task LoadItemsWithPagingAndOrderingWorks();


    //    Task LoadItemsWithWhereWorks();

    //    void LoadItemsWithCancelationThrowsError();

    //    void LoadItemThrowsErrorWithKeyNullOrEmpty();

    //    void LoadItemWithCancelationThrowsError();

    //    Task SaveAndLoadItemPropertiesEquals();

    //    Task SaveAndLoadItemsContainsAll();

    //    Task SaveAndUpdateItemPropertiesEquals();

    //    void SaveThrowsErrorWithItemNull();

    //    void UpdateWithCancelationThrowsError();
    //}

    public abstract class EntityStoreTestsBase : TestBase
    {
        protected EntityStoreTestsBase(ITestOutputHelper output) : base(output)
        {
            ServiceCollection.AddSingleton<ICacheClient>(new NullCacheClient());
        }


        public virtual async Task CanInsertAndDeleteItem()
        {
            await InitializeAsync();

            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();


            await store.InsertItemAsync(obj1);

            obj1.StringValue = "changed string";
            await store.UpdateItemAsync(obj1);

            var obj2 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);
            CompareEntitities(obj1, obj2);

            await store.DeleteItemAsync<FakeEntity>(obj1.EntityId);

            var obj3 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);
            obj3.Should().BeNull();
        }


        public virtual async Task CanInsertAndDeleteItemWithWhere()
        {
            await InitializeAsync();

            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();

            await store.InsertItemAsync(obj1);

            obj1.StringValue = "changed string";
            await store.UpdateItemAsync(obj1);

            var obj2 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);
            CompareEntitities(obj1, obj2);

            await store.DeleteItemsAsync<FakeEntity>(entity => entity.EntityId == obj1.EntityId);

            var obj3 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);
            obj3.Should().BeNull();
        }

        public virtual async Task CanInsertAndDeleteAllItems()
        {
            await InitializeAsync();

            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();

            await store.InsertItemAsync(obj1);

            obj1.StringValue = "changed string";
            await store.UpdateItemAsync(obj1);

            var obj2 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);
            CompareEntitities(obj1, obj2);

            await store.DeleteAllItemsAsync<FakeEntity>();

            var obj3 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);
            obj3.Should().BeNull();
        }


        public virtual async Task CanInsertAndUpdateItem()
        {
            await InitializeAsync();

            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();

            await store.InsertItemAsync(obj1);

            obj1.StringValue = "changed string";
            await store.UpdateItemAsync(obj1);


            var obj2 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);
            CompareEntitities(obj1, obj2);
        }


        public virtual async Task CanInsertItem()
        {
            await InitializeAsync();

            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();

            await store.InsertItemAsync(obj1);

            var obj2 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);

            CompareEntitities(obj1, obj2);
        }

        public virtual async Task CanHandleDictionaries()
        {
            await InitializeAsync();

            var store = CreateInstance();

            string entityId;

            {
                var id = Guid.NewGuid().ToString();
                var entity = FakeEntity.Create(id);
                entityId = entity.EntityId;
                entityId.Should().NotBeNullOrEmpty();

                var aaa = DateTime.UtcNow.Ticks.ToString();
                entity.Dict1 = entity.Dict1.EnsureDictContainsValue("aaa", aaa);

                var bbb = DateTime.UtcNow.Ticks.ToString();
                entity.Dict1 = entity.Dict1.EnsureDictContainsValue("bbb", bbb);

                await store.InsertItemAsync(entity);

                entity.Dict1.Should().ContainKey("aaa");
                entity.Dict1["aaa"].Should().Be(aaa);
                
                entity.Dict1.Should().ContainKey("bbb");
                entity.Dict1["bbb"].Should().Be(bbb);
            }

            {
                var entity = await store.LoadItemAsync<FakeEntity>(entityId);
                
                var ccc = DateTime.UtcNow.Ticks.ToString();
                entity.Dict1 = entity.Dict1.EnsureDictContainsValue("ccc", ccc);
                await store.UpdateItemAsync(entity);

                entity.Dict1.Should().ContainKey("ccc");
                entity.Dict1["ccc"].Should().Be(ccc);

                entity = await store.LoadItemAsync<FakeEntity>(entityId);

                entity.Dict1.Should().ContainKey("ccc");
                entity.Dict1["ccc"].Should().Be(ccc);
            }

            {
                var entity = await store.LoadItemAsync<FakeEntity>(entityId);
             
                var bbb = DateTime.UtcNow.Ticks.ToString();
                entity.Dict1 = entity.Dict1.EnsureDictContainsValue("bbb", bbb);

                entity.Dict1.Should().ContainKey("bbb");
                entity.Dict1["bbb"].Should().Be(bbb);

                var aaa = DateTime.UtcNow.Ticks.ToString();
                entity.Dict1 = entity.Dict1.EnsureDictContainsValue("aaa", aaa);

                await store.UpdateItemAsync(entity);

                entity.Dict1.Should().ContainKey("aaa");
                entity.Dict1["aaa"].Should().Be(aaa);
                
                entity = await store.LoadItemAsync<FakeEntity>(entityId);
                entity.Dict1.Should().ContainKey("aaa");
                
                entity.Dict1.Should().ContainKey("bbb");
                entity.Dict1["bbb"].Should().Be(bbb);entity.Dict1["aaa"].Should().Be(aaa);

            }





        }

        public virtual async Task CanInsertManyItems()
        {
            await InitializeAsync();

            var list = new List<FakeEntity>();

            for (var i = 0; i < 1000; i++) list.Add(FakeEntity.Create(Guid.NewGuid().ToString()));


            var store = CreateInstance();

            foreach (var entity in list) await store.InsertItemAsync(entity);

            var all = await store.LoadItemsAsync<FakeEntity>();
            var allKeys = all.Select(x => x.EntityId).ToArray();

            foreach (var entity in list) allKeys.Should().Contain(entity.EntityId);
        }

        public virtual async Task CanBulkInsertManyItems()
        {
            await InitializeAsync();

            var list = new List<FakeEntity>();

            for (var i = 0; i < BulkInsertCount; i++) list.Add(FakeEntity.Create(Guid.NewGuid().ToString()));

            var store = CreateInstance();

            await store.BulkInsertItemsAsync(list);

            var all = await store.LoadItemsAsync<FakeEntity>();
            var allKeys = all.Select(x => x.EntityId).ToArray();

            foreach (var entity in list) allKeys.Should().Contain(entity.EntityId);
        }

        public int BulkInsertCount { get; set; } = 1000;


        public virtual void DeleteThrowsErrorWithKeyNullOrEmpty()
        {
            var store = CreateInstance();

            Func<Task> f1 = async () => { await store.DeleteItemAsync<FakeEntity>(null); };
            f1.Should().Throw<ArgumentNullException>();

            Func<Task> f2 = async () => { await store.DeleteItemAsync<FakeEntity>(string.Empty); };
            f2.Should().Throw<ArgumentNullException>();
        }

        public virtual void DeleteWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);

            Func<Task> f1 = async () =>
            {
                await store.DeleteItemAsync<FakeEntity>(Guid.NewGuid().ToString(), token);
            };

            f1.Should().Throw<Exception>();
        }

        public virtual void InsertWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            Func<Task> f1 = async () =>
            {
                await store.InsertItemAsync(new FakeEntity(), token);
            };

            f1.Should().Throw<Exception>();
        }


        //public virtual async Task LoadItemsWithPagingWorks()
        //{
        //    await InitializeAsync();
        //    var store = CreateInstance();

        //    var list = FakeEntity.CreateList(31).ToArray();

        //    foreach (var entity in list)
        //        await store.InsertItemAsync(entity);


        //    //page 1
        //    var page1 = await store.LoadPagedResultAsync(
        //        new PagedLoadingParameters<FakeEntity>
        //        {
        //            OrderBy = x => x.IntValue,
        //            Where = x => x.ChunkId == list[0].ChunkId,
        //            Page = 1,
        //            PageSize = 10
        //        }
        //    );

        //    page1.TotalCount.Should().Be(31);
        //    page1.PageSize.Should().Be(10);
        //    page1.ResultCount.Should().Be(10);
        //    page1.Results.Count.Should().Be(10);


        //    //page 2
        //    var page2 = await store.LoadPagedResultAsync(
        //        new PagedLoadingParameters<FakeEntity>
        //        {
        //            OrderBy = x => x.IntValue,
        //            Where = x => x.ChunkId == list[0].ChunkId,
        //            Page = 2,
        //            PageSize = 10,
        //            ContinuationToken = page1.ContinuationToken
        //        }
        //    );


        //    page2.TotalCount.Should().Be(31);
        //    page2.PageSize.Should().Be(10);
        //    page2.ResultCount.Should().Be(10);
        //    page2.Results.Count.Should().Be(10);

        //    //page 2
        //    var page3 = await store.LoadPagedResultAsync(
        //        new PagedLoadingParameters<FakeEntity>
        //        {
        //            OrderBy = x => x.IntValue,
        //            Where = x => x.ChunkId == list[0].ChunkId,
        //            Page = 3,
        //            PageSize = 10,
        //            ContinuationToken = page2.ContinuationToken
        //        }
        //    );

        //    page3.TotalCount.Should().Be(31);
        //    page3.PageSize.Should().Be(10);
        //    page3.ResultCount.Should().BeGreaterOrEqualTo(1);
        //    page3.Results.Count.Should().BeGreaterOrEqualTo(1);
        //}

        //public virtual void LoadItemsWithPagingAndCancelationThrowsError()
        //{
        //    var store = CreateInstance();

        //    var token = new CancellationToken(true);
        //    Func<Task> f1 = async () =>
        //        {
        //            await store.LoadPagedResultAsync(new PagedLoadingParameters<FakeEntity>(), token);
        //        };

        //    f1.Should().Throw<Exception>();
        //}

        //public virtual async Task LoadItemsWithPagingAndOrderingWorks()
        //{
        //    var store = CreateInstance();

        //    var list = FakeEntity.CreateList2(31).ToArray();

        //    foreach (var entity in list)
        //        await store.InsertItemAsync(entity);

        //    //page 1
        //    var page1 = await store.LoadPagedResultAsync(
        //        new PagedLoadingParameters<FakeEntity>
        //        {
        //            OrderBy = x => x.IntValue,
        //            OrderThenBy = x => x.StringValue,
        //            Where = x => x.ChunkId == list[0].ChunkId,
        //            Page = 1,
        //            PageSize = 10
        //        }
        //    );

        //    page1.TotalCount.Should().Be(31);
        //    page1.PageSize.Should().Be(10);
        //    page1.ResultCount.Should().Be(10);
        //    page1.Results.Count.Should().Be(10);

        //    page1.Results[0].StringValue.Should().Be(list.Last().StringValue);
        //}


        public virtual async Task LoadItemsWithWhereWorks()
        {
            await InitializeAsync();
            var store = CreateInstance();

            var list = FakeEntity.CreateList(31).ToArray();

            foreach (var entity in list)
                await store.InsertItemAsync(entity);

            var chunkId = list[0].ChunkId;

            var items = (await store.LoadItemsAsync<FakeEntity>(x => x.ChunkId == chunkId)).ToArray();
            items.Should().NotBeNull();
            items.Length.Should().Be(list.Length);

            var guidVal = list[0].GuidValue;
            var items1 = (await store.LoadItemsAsync<FakeEntity>(x => x.ChunkId == chunkId && x.GuidValue == guidVal)).ToArray();
            items1.Length.Should().Be(1);
        }

        public virtual void LoadItemsWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            Func<Task> f1 = async () => { await store.LoadItemsAsync<FakeEntity>(entity => true, token); };
            f1.Should().Throw<Exception>();
        }


        public virtual void LoadItemThrowsErrorWithKeyNullOrEmpty()
        {
            var store = CreateInstance();

            Func<Task> f1 = async () =>
            {
                var e = await store.LoadItemAsync<FakeEntity>(null);
                e.Should().NotBeNull();
            };

            f1.Should().Throw<ArgumentNullException>();

            Func<Task> f2 = async () =>
            {
                var e = await store.LoadItemAsync<FakeEntity>(string.Empty);
                e.Should().NotBeNull();
            };
            f2.Should().Throw<ArgumentNullException>();
        }

        public virtual void LoadItemWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            Func<Task> f1 = async () =>
            {
                await store.LoadItemAsync<FakeEntity>(Guid.NewGuid().ToString(), token);
            };

            f1.Should().Throw<Exception>();
        }

        public virtual async Task SaveAndLoadItemPropertiesEquals()
        {
            await InitializeAsync();
            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();

            await store.InsertItemAsync(obj1);

            var entityId = id.CalculateEntityId<FakeEntity>();
            var obj2 = await store.LoadItemAsync<FakeEntity>(entityId);

            CompareEntitities(obj1, obj2);
        }


        public virtual async Task SaveAndLoadItemsContainsAll()
        {
            await InitializeAsync();
            var obj1 = FakeEntity.Create(Guid.NewGuid().ToString());
            var obj2 = FakeEntity.Create(Guid.NewGuid().ToString());

            var store = CreateInstance();
            await store.InsertItemAsync(obj1);
            await store.InsertItemAsync(obj2);

            var all = await store.LoadItemsAsync<FakeEntity>();
            var allKeys = all.Select(x => x.EntityId).ToArray();
            allKeys.Should().Contain(obj1.EntityId);
            allKeys.Should().Contain(obj2.EntityId);
        }


        public virtual async Task SaveAndUpdateItemPropertiesEquals()
        {
            await InitializeAsync();
            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();

            await store.InsertItemAsync(obj1);

            obj1.GuidValue = Guid.NewGuid();
            await store.UpdateItemAsync(obj1);

            var entityId = id.CalculateEntityId<FakeEntity>();
            var obj2 = await store.LoadItemAsync<FakeEntity>(entityId);

            CompareEntitities(obj1, obj2);
        }


        public virtual void SaveThrowsErrorWithItemNull()
        {

            var store = CreateInstance();

            Func<Task> f1 = async () => { await store.UpdateItemAsync<FakeEntity>(null); };
            f1.Should().Throw<ArgumentNullException>();

            Func<Task> f2 = async () => { await store.InsertItemAsync<FakeEntity>(null); };
            f2.Should().Throw<ArgumentNullException>();

            Func<Task> f3 = async () => { await store.InsertOrUpdateItemAsync<FakeEntity>(null); };
            f3.Should().Throw<ArgumentNullException>();
        }

        public virtual void UpdateWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);

            Func<Task> f1 = async () => { await store.UpdateItemAsync(new FakeEntity(), token); };
            f1.Should().Throw<Exception>();
        }

        protected IEntityStore CreateInstance()
        {
            return GetService<IEntityStore>();
        }

        protected virtual async Task InitializeAsync()
        {
            var provider = GetService<IEntityStoreConnectionProvider>();
            await provider.InitializeAsync(CancellationToken.None);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected static void CompareEntitities(FakeEntity obj1, FakeEntity obj2)
        {
            obj2.EntityId.Should().Be(obj1.EntityId);
            obj2.DateTimeValueUtc.Should().Be(obj1.DateTimeValueUtc);
            obj2.DoubleValue.Should().Be(obj1.DoubleValue);
            obj2.GuidValue.Should().Be(obj1.GuidValue);
            obj2.StringValue.Should().Be(obj1.StringValue);
        }


        public virtual async Task InsertExistingWillUpdate()
        {
            await InitializeAsync();
            var id = Guid.NewGuid().ToString();

            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();
            await store.InsertItemAsync(obj1);

            var obj2 = FakeEntity.Create(id);
            obj2.StringValue = Guid.NewGuid().ToString();
            await store.InsertItemAsync(obj2);

            obj1 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);

            CompareEntitities(obj1, obj2);
        }

        public virtual async Task UpdateNonExistingWillInsert()
        {
            await InitializeAsync();
            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();
            await store.UpdateItemAsync(obj1);

            var obj2 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);

            CompareEntitities(obj1, obj2);
        }
    }
}