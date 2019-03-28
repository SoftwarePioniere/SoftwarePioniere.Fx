using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
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

        [Fact]
        public void CanParseWrappedMessage()
        {
            var ev1 = FakeEvent.Create();

            var dict1 = new Dictionary<string, string>
            {
                {"k1", "v1"},
                {"k2", "v2"}
            };
            var msg1 = ev1.CreateWrappedMessage(dict1);

            msg1.Should().NotBeNull();

            msg1.Properties.Should().ContainKey("k1");
            msg1.Properties.Should().ContainKey("k2");


            var json1 = JsonConvert.SerializeObject(msg1);
            json1.Should().NotBeNullOrEmpty();

            var msg2 = JsonConvert.DeserializeObject<WrappedMessage<FakeEvent>>(json1);

            msg2.Should().NotBeNull();

            msg2.Properties.Should().ContainKey("k1");
            msg2.Properties.Should().ContainKey("k2");
        }



    }
}
