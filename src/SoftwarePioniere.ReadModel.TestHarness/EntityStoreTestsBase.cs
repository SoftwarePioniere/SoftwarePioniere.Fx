using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Foundatio.Caching;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace SoftwarePioniere.ReadModel
{
    public interface IEntityStoreTests
    {
        Task CanInsertAndDeleteItem();

        Task CanInsertAndUpdateItem();

        Task CanInsertItem();

        Task CanInsertManyItems();

        void DeleteThrowsErrorWithKeyNullOrEmpty();

        void DeleteWithCancelationThrowsError();

        void InsertWithCancelationThrowsError();

        Task LoadItemsWithPagingWorks();

        void LoadItemsWithPagingAndCancelationThrowsError();

        //Task LoadItemsWithPagingAndOrderingWorks();


        Task LoadItemsWithWhereWorks();

        void LoadItemsWithCancelationThrowsError();

        void LoadItemThrowsErrorWithKeyNullOrEmpty();

        void LoadItemWithCancelationThrowsError();

        Task SaveAndLoadItemPropertiesEquals();

        Task SaveAndLoadItemsContainsAll();

        Task SaveAndUpdateItemPropertiesEquals();

        void SaveThrowsErrorWithItemNull();

        void UpdateWithCancelationThrowsError();
    }

    public abstract class EntityStoreTestsBase : TestBase, IEntityStoreTests
    {
        protected EntityStoreTestsBase(ITestOutputHelper output) : base(output)
        {
            ServiceCollection.AddSingleton<ICacheClient>(new InMemoryCacheClient());
        }


        public virtual async Task CanInsertAndDeleteItem()
        {
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


        public virtual async Task CanInsertAndUpdateItem()
        {
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
            var id = Guid.NewGuid().ToString();
            var obj1 = FakeEntity.Create(id);

            var store = CreateInstance();

            await store.InsertItemAsync(obj1);

            var obj2 = await store.LoadItemAsync<FakeEntity>(obj1.EntityId);

            CompareEntitities(obj1, obj2);
        }

        public virtual async Task CanInsertManyItems()
        {
            var list = new List<FakeEntity>();

            for (var i = 0; i < 1000; i++) list.Add(FakeEntity.Create(Guid.NewGuid().ToString()));


            var store = CreateInstance();

            foreach (var entity in list) await store.InsertItemAsync(entity);

            var all = await store.LoadItemsAsync<FakeEntity>();
            var allKeys = all.Select(x => x.EntityId).ToArray();

            foreach (var entity in list) allKeys.Should().Contain(entity.EntityId);
        }


        public virtual void DeleteThrowsErrorWithKeyNullOrEmpty()
        {
            var store = CreateInstance();

            var act1 = new Action(() => store.DeleteItemAsync<FakeEntity>(null).Wait());
            act1.Should().Throw<ArgumentNullException>();

            var act2 = new Action(() => store.DeleteItemAsync<FakeEntity>(string.Empty).Wait());
            act2.Should().Throw<ArgumentNullException>();
        }

        public virtual void DeleteWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            var act1 = new Action(() =>
                store.DeleteItemAsync<FakeEntity>(Guid.NewGuid().ToString(), token).Wait(token));
            act1.Should().Throw<Exception>();
        }

        public virtual void InsertWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            var act1 = new Action(() => store.InsertItemAsync(new FakeEntity(), token).Wait(token));
            act1.Should().Throw<Exception>();
        }


        public virtual async Task LoadItemsWithPagingWorks()
        {
            var store = CreateInstance();

            var list = FakeEntity.CreateList(31).ToArray();

            foreach (var entity in list)
                await store.InsertItemAsync(entity);


            //page 1
            var page1 = await store.LoadPagedResultAsync(
                new PagedLoadingParameters<FakeEntity>
                {
                    OrderBy = x => x.IntValue,
                    Where = x => x.ChunkId == list[0].ChunkId,
                    Page = 1,
                    PageSize = 10
                }
            );

            page1.TotalCount.Should().Be(31);
            page1.PageSize.Should().Be(10);
            page1.ResultCount.Should().Be(10);
            page1.Results.Count.Should().Be(10);


            //page 2
            var page2 = await store.LoadPagedResultAsync(
                new PagedLoadingParameters<FakeEntity>
                {
                    OrderBy = x => x.IntValue,
                    Where = x => x.ChunkId == list[0].ChunkId,
                    Page = 2,
                    PageSize = 10,
                    ContinuationToken = page1.ContinuationToken
                }
            );


            page2.TotalCount.Should().Be(31);
            page2.PageSize.Should().Be(10);
            page2.ResultCount.Should().Be(10);
            page2.Results.Count.Should().Be(10);

            //page 2
            var page3 = await store.LoadPagedResultAsync(
                new PagedLoadingParameters<FakeEntity>
                {
                    OrderBy = x => x.IntValue,
                    Where = x => x.ChunkId == list[0].ChunkId,
                    Page = 3,
                    PageSize = 10,
                    ContinuationToken = page2.ContinuationToken
                }
            );

            page3.TotalCount.Should().Be(31);
            page3.PageSize.Should().Be(10);
            page3.ResultCount.Should().BeGreaterOrEqualTo(1);
            page3.Results.Count.Should().BeGreaterOrEqualTo(1);
        }

        public virtual void LoadItemsWithPagingAndCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            var act1 = new Action(() => store.LoadPagedResultAsync(new PagedLoadingParameters<FakeEntity>(), token).Wait(token));
            act1.Should().Throw<Exception>();
        }

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
            var store = CreateInstance();

            var list = FakeEntity.CreateList(31).ToArray();

            foreach (var entity in list)
                await store.InsertItemAsync(entity);

            var chunkId = list[0].ChunkId;

            var items = await store.LoadItemsAsync<FakeEntity>(x => x.ChunkId == chunkId);
            items.Should().NotBeNull();
            items.Length.Should().Be(list.Length);

            var guidVal = list[0].GuidValue;
            var items1 = await store.LoadItemsAsync<FakeEntity>(x => x.ChunkId == chunkId && x.GuidValue == guidVal);
            items1.Length.Should().Be(1);
        }

        public virtual void LoadItemsWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            var act1 = new Action(() => store.LoadItemsAsync<FakeEntity>(entity => true, token).Wait(token));
            act1.Should().Throw<Exception>();
        }


        public virtual void LoadItemThrowsErrorWithKeyNullOrEmpty()
        {
            var store = CreateInstance();

            var act1 = new Action(() =>
            {
                var e = store.LoadItemAsync<FakeEntity>(null).Result;
                e.Should().NotBeNull();
            });

            act1.Should().Throw<ArgumentNullException>();

            var act2 = new Action(() =>
            {
                var e = store.LoadItemAsync<FakeEntity>(string.Empty).Result;
                e.Should().NotBeNull();
            });

            act2.Should().Throw<ArgumentNullException>();
        }

        public virtual void LoadItemWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            var act1 = new Action(() => store.LoadItemAsync<FakeEntity>(Guid.NewGuid().ToString(), token).Wait(token));
            act1.Should().Throw<Exception>();
        }

        public virtual async Task SaveAndLoadItemPropertiesEquals()
        {
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

            var act1 = new Action(() => store.UpdateItemAsync<FakeEntity>(null).Wait());
            act1.Should().Throw<AggregateException>().WithInnerException<ArgumentNullException>();

            var act2 = new Action(() => store.InsertItemAsync<FakeEntity>(null).Wait());
            act2.Should().Throw<ArgumentNullException>();

            var act3 = new Action(() => store.InsertOrUpdateItemAsync<FakeEntity>(null).Wait());
            act3.Should().Throw<ArgumentNullException>();
        }

        public virtual void UpdateWithCancelationThrowsError()
        {
            var store = CreateInstance();

            var token = new CancellationToken(true);
            var act1 = new Action(() => store.UpdateItemAsync(new FakeEntity(), token).Wait(token));
            act1.Should().Throw<Exception>();
        }

        protected IEntityStore CreateInstance()
        {
            return GetService<IEntityStore>();
        }


        private static void CompareEntitities(FakeEntity obj1, FakeEntity obj2)
        {
            obj2.EntityId.Should().Be(obj1.EntityId);
            obj2.DateTimeValueUtc.Should().Be(obj1.DateTimeValueUtc);
            obj2.DoubleValue.Should().Be(obj1.DoubleValue);
            obj2.GuidValue.Should().Be(obj1.GuidValue);
            obj2.StringValue.Should().Be(obj1.StringValue);
        }
    }
}