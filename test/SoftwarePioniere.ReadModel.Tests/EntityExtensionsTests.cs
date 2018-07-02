using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.ReadModel.Tests
{
    public class EntityExtensionsTests
    {
        private const string EntityId = "My:FakeEntity-fakeid";

        [Fact]
        public void CanCalculateEntityIdFromType()
        {
            var entid = "fakeid".CalculateEntityId<FakeEntity>(); 
            entid.Should().Be(EntityId);
        }

        [Fact]
        public void CanCalculateEntityIdFromInstance()
        {
            var ent = new FakeEntity();
            var entid =  ent.CalculateEntityId("fakeid");

            entid.Should().Be(EntityId);
        }

        [Fact]
        public void CanSetEntityId()
        {
            var ent = new FakeEntity();
            ent.SetEntityId("fakeid");

            ent.EntityId.Should().Be(EntityId);
        }
    }
}
