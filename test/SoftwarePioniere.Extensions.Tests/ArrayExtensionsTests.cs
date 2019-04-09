using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.Extensions.Tests
{
    public class ArrayExtensionsTests
    {
        [Fact]
        public void GetArrayTest()
        {
            string[] arr = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            arr.GetArray().Should().NotBeNull();
            // ReSharper disable once ExpressionIsAlwaysNull
            arr.GetArray().Length.Should().Be(0);

            arr = new[] { "a", "b" };
            arr.GetArray().Should().NotBeNull();
            arr.GetArray().Length.Should().Be(2);
        }

        [Fact]
        public void EnsureArrayContainsValueTest()
        {
            var arr = new[] { "a", "b" };
            arr = arr.EnsureArrayContainsValue("c");
            arr.Should().Contain("a");
            arr.Should().Contain("b");
            arr.Should().Contain("c");
        }

        [Fact]
        public void EnsureArrayNotContainsValueTest()
        {
            var arr = new[] { "a", "b", "c" };
            arr = arr.EnsureArrayNotContainsValue("c");
            arr.Should().Contain("a");
            arr.Should().Contain("b");
            arr.Should().NotContain("c");
        }
    }
}
