using FluentAssertions;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.ReadModel;
using Xunit;

namespace SoftwarePioniere.Tests.ReadModel
{
    public class AssemblyExtensionsTests
    {
        [Fact]
        public void EntityTypesConstantsShouldContainMyEntity()
        {
            var assembly = typeof(FakeEntity).Assembly;
            var entityTypes = assembly.GetEntityTypesConstants();

            entityTypes.Should().Contain(x => x == "My:FakeEntity".ToUpper());
        }

        [Fact]
        public void EntityTypesInfosShouldContainMyEntity()
        {

            var et = typeof(FakeEntity);
            var assembly = et.Assembly;
            var entityTypes = assembly.GetEntityTypeInfos();

            entityTypes.Should().Contain(x =>
                x.TypeKey == "My:FakeEntity".ToUpper()
                && x.Name == et.Name
                && x.FullName == et.FullName
            );
        }
    }
}
