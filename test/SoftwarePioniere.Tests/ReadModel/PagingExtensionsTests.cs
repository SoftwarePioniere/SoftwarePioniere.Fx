using System.Linq;
using FluentAssertions;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.ReadModel;
using Xunit;

namespace SoftwarePioniere.Tests.ReadModel
{
    public class PagingExtensionsTests
    {
        [Fact]
        public void TestGetPaged()
        {
            var items = FakeEntity.CreateList(10).ToArray();

            var paged = items.GetPaged(3, 2);
            paged.Length.Should().Be(3);

            paged.Should().Contain(x => x.EntityId == items[3].EntityId);
            paged.Should().Contain(x => x.EntityId == items[4].EntityId);
            paged.Should().Contain(x => x.EntityId == items[5].EntityId);
        }

        [Fact]
        public void TestGetPagedResults()
        {
            var items = FakeEntity.CreateList(10).ToArray();

            var paged = items.GetPagedResults(3, 2);
            paged.PageSize.Should().Be(3);
            paged.TotalCount.Should().Be(items.Length);

            paged.Results.Should().Contain(x => x.EntityId == items[3].EntityId);
            paged.Results.Should().Contain(x => x.EntityId == items[4].EntityId);
            paged.Results.Should().Contain(x => x.EntityId == items[5].EntityId);
        }
    }
}
