using System.Text;
using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.Extensions.Tests
{
    public class StreamExtensionsTests
    {
        [Fact]
        public void StreamTest()
        {
            const string s = "Hallo";

            var byts = Encoding.UTF8.GetBytes(s);
            var stream = byts.CreateStream();

            var byts2 = stream.CreateByteArray();
            var s2 = Encoding.UTF8.GetString(byts2);

            s2.Should().Be(s);
        }
    }
}