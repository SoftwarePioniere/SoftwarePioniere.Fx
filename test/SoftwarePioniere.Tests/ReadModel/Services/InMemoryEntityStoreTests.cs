using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.ReadModel;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.Tests.ReadModel.Services
{
    public class InMemoryEntityStoreTests : EntityStoreTestsBase
    {
        public InMemoryEntityStoreTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection.AddInMemoryEntityStore();
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
        public override Task CanInsertManyItems()
        {
            return base.CanInsertManyItems();
        }

        [Fact]
        public override Task CanBulkInsertManyItems()
        {
            return base.CanBulkInsertManyItems();
        }

        [Fact]
        public override void DeleteThrowsErrorWithKeyNullOrEmpty()
        {
            base.DeleteThrowsErrorWithKeyNullOrEmpty();
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

        [Fact]
        public override void LoadItemsWithCancelationThrowsError()
        {
            base.LoadItemsWithCancelationThrowsError();
        }

        [Fact]
        public override void LoadItemsWithPagingAndCancelationThrowsError()
        {
            base.LoadItemsWithPagingAndCancelationThrowsError();
        }

        [Fact]
        public override Task LoadItemsWithPagingWorks()
        {
            return base.LoadItemsWithPagingWorks();
        }

        //[Fact]
        //public override Task LoadItemsWithPagingAndOrderingWorks()
        //{
        //    return base.LoadItemsWithPagingAndOrderingWorks();
        //}

        [Fact]
        public override Task LoadItemsWithWhereWorks()
        {
            return base.LoadItemsWithWhereWorks();
        }

        [Fact]
        public override void LoadItemThrowsErrorWithKeyNullOrEmpty()
        {
            base.LoadItemThrowsErrorWithKeyNullOrEmpty();
        }

        [Fact]
        public override void LoadItemWithCancelationThrowsError()
        {
            base.LoadItemWithCancelationThrowsError();
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
        public override void UpdateWithCancelationThrowsError()
        {
            base.UpdateWithCancelationThrowsError();
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
        public override Task InsertExistingWillUpdate()
        {
            return base.InsertExistingWillUpdate();
        }
     
        [Fact]
        public override Task UpdateNonExistingWillInsert()
        {
            return base.UpdateNonExistingWillInsert();
        }
    }
}