using FluentAssertions;
using SoftwarePioniere.Messaging.Notifications;
using Xunit;

namespace SoftwarePioniere.Messaging.Tests
{
    public class AssemblyExtensionsTests
    {
        [Fact]
        public void NotificationContentTypesConstantsReadModelUpdated()
        {
            var assembly = typeof(ReadModelUpdatedNotification).Assembly;
            var entityTypes = assembly.GetNotificationContentTypesConstants();

            entityTypes.Should().Contain(x => x == "readmodel.updated".ToUpper());
        }

        [Fact]
        public void EntityTypesInfosShouldContainMyEntity()
        {

            var et = typeof(ReadModelUpdatedNotification);
            var assembly = et.Assembly;
            var entityTypes = assembly.GetNotificationContentTypesInfos();

            entityTypes.Should().Contain(x =>
                x.TypeKey == "readmodel.updated".ToUpper()
                && x.Name == et.Name
                && x.FullName == et.FullName
            );
        }
    }
}
