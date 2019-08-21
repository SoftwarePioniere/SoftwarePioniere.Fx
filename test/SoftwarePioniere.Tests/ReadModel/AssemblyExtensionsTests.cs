using FluentAssertions;
using SoftwarePioniere.ReadModel;
using Xunit;

namespace SoftwarePioniere.Tests.ReadModel
{
    public class AssemblyExtensionsTests
    {
        [Fact]
        public void EntityTypesConstantsShouldContainMyEntity()
        {
            var assembly = typeof(MyEntity1).Assembly;
            var entityTypes = assembly.GetEntityTypesConstants();

            entityTypes.Should().Contain(x => x == "My:Entity1".ToUpper());
        }

        [Fact]
        public void EntityTypesInfosShouldContainMyEntity()
        {

            var et = typeof(MyEntity1);
            var assembly = et.Assembly;
            var entityTypes = assembly.GetEntityTypeInfos();

            entityTypes.Should().Contain(x =>
                x.TypeKey == "My:Entity1".ToUpper()
                && x.Name == et.Name
                && x.FullName == et.FullName
            );
        }
    }
}
