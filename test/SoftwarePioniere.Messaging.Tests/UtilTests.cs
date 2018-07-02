using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.Messaging.Tests
{
    public class UtilTests
    {
        [Fact]
        public void GetEventNameTest()
        {
            var name = typeof(FakeEvent).GetEventName();
            name.Should().Be("FakeEvent");

        }

        [Fact]
        public void EventTypeToEventNameTest()
        {
            var name = Util.EventTypeToEventName(typeof(FakeEvent));
            name.Should().Be("FakeEvent");
        }


        [Fact]
        public void CanCreateTypeFromShortName()
        {
            const string typeShortName = " SoftwarePioniere.Messaging.FakeEvent, SoftwarePioniere.Messaging.TestHarness";

            var t = Util.CreateType(typeShortName);

            t.Should().NotBeNull();
            t.FullName.Should().Be(typeof(FakeEvent).FullName);
        }

        [Fact]
        public void CanCreateTypeFromShortNameWithWrongAssembly()
        {
            const string typeShortName = " SoftwarePioniere.Messaging.FakeEvent, SoftwarePioniere.Messaging.TestHarness.notfound";

            var t = Util.CreateType(typeof(FakeEvent).Assembly, typeShortName);

            t.Should().NotBeNull();
            t.FullName.Should().Be(typeof(FakeEvent).FullName);
        }
    }
}
