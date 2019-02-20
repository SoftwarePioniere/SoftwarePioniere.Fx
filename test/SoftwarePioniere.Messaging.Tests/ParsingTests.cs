using System;
using FluentAssertions;
using Xunit;

namespace SoftwarePioniere.Messaging.Tests
{
    public class ParsingTests
    {
        [Fact]
        public void CanParseToIMessage()
        {
            object message = FakeEvent.Create();
            Type messageType = message.GetType();

            var isIMessage = typeof(IMessage).IsAssignableFrom(messageType);

            isIMessage.Should().BeTrue();

        }
    }
}
