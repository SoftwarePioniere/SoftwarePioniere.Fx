using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.ReadModel.Tests
{
    public class CacheKeysTests
    {
        [Fact]
        public void CacheKeyTest()
        {
            var strings = new List<string> { "s1", "s2" };
            CacheKeys.Create<MyEntity1>().Should().Be(MyEntity1.TypeKey);
            CacheKeys.Create<MyEntity1>(strings).Should().Be($"{MyEntity1.TypeKey}.s1.s2");

            CacheKeys.Create<MyEntity1>("s1").Should().Be($"{MyEntity1.TypeKey}.s1");
            CacheKeys.Create<MyEntity1>("s1", 1, 2).Should().Be($"{MyEntity1.TypeKey}.s1.1.2");
            CacheKeys.Create<MyEntity1>(1, 2, strings).Should().Be($"{MyEntity1.TypeKey}.1.2.s1.s2");
            CacheKeys.Create<MyEntity1>("sx", 1, 2, strings).Should().Be($"{MyEntity1.TypeKey}.sx.1.2.s1.s2");
            CacheKeys.Create<MyEntity1>("sx", strings).Should().Be($"{MyEntity1.TypeKey}.sx.s1.s2");
            CacheKeys.Create<MyEntity1>("sx", strings, strings).Should().Be($"{MyEntity1.TypeKey}.sx.s1.s2.s1.s2");
            CacheKeys.Create<MyEntity1>("sx", "sy", strings).Should().Be($"{MyEntity1.TypeKey}.sx.sy.s1.s2");
            CacheKeys.Create<MyEntity1>("sx", "sy").Should().Be($"{MyEntity1.TypeKey}.sx.sy");
            CacheKeys.Create<MyEntity1>("sx", "sy", "sz").Should().Be($"{MyEntity1.TypeKey}.sx.sy.sz");
            CacheKeys.Create<MyEntity1>("sx", "sy", "sz", "szz").Should().Be($"{MyEntity1.TypeKey}.sx.sy.sz.szz");
            CacheKeys.Create<MyEntity1>("sx", "sy", "sz", strings).Should().Be($"{MyEntity1.TypeKey}.sx.sy.sz.s1.s2");
            CacheKeys.Create<MyEntity1>("sx", "sy", 1, 2).Should().Be($"{MyEntity1.TypeKey}.sx.sy.1.2");
            CacheKeys.Create<MyEntity1>("sx", "sy", 1, 2, strings).Should().Be($"{MyEntity1.TypeKey}.sx.sy.1.2.s1.s2");
            CacheKeys.Create<MyEntity1>("sx", "sy", "sz", 1, 2).Should().Be($"{MyEntity1.TypeKey}.sx.sy.sz.1.2");
           
        }
    }
}
