using System;
using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.Extensions.Tests
{
    public class EnumExtensionsTests
    {
        [Fact]
        public void ParseEnumTest()
        {
            var x = "Sunday".ParseEnum<DayOfWeek>();
            x.Should().Be(DayOfWeek.Sunday);
        }
    }
}