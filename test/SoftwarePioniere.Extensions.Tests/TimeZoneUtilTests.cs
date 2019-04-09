using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.Extensions.Tests
{
    public class TimeZoneUtilTests
    {
        [Fact]
        public void GetHomeTimeZoneTest()
        {
            var tz = TimeZoneUtil.GetHomeTimeZone();
            tz.Should().NotBeNull();

            TimeZoneUtil.HomeZones.Should().Contain(tz.Id);

        }
    }
}
