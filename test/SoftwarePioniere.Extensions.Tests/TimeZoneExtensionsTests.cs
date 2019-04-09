using System;
using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.Extensions.Tests
{
    public class TimeZoneExtensionsTests
    {
        [Fact]
        public void GetAllDaysTest()
        {
            var tz = TimeZoneUtil.GetHomeTimeZone();

            var cur = DateTime.UtcNow.Date;

            var days = tz.GetAllDays(6);
            days.Should().NotBeNull();
            days.Length.Should().Be(7);
            days[0].Date.Should().Be(cur.Date);
            days[6].Date.Should().Be(cur.Date.AddDays(6).Date);

        }
    }

   
}