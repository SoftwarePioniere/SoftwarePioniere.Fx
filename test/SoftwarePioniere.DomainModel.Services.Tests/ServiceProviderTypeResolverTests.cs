using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.DomainModel.FakeDomain;
using Xunit;
using Xunit.Abstractions;

namespace SoftwarePioniere.DomainModel.Services.Tests
{
    public class ServiceProviderTypeResolverTests : TestBase
    {
        [Fact]
        public void CanResolveInstanceOfType()
        {
            ServiceCollection               
                .AddTransient<FakeAggregate>();

            var resolver = GetService<IResolveType>();
            resolver.Should().NotBeNull();

            var agg = resolver.Resolve(typeof(FakeAggregate));
            agg.Should().NotBeNull();

        }

        [Fact]
        public void ResolveOfUnregisteredTypeThrowsError()
        {
            var resolver = GetService<IResolveType>();
            resolver.Should().NotBeNull();

            Action act = () => resolver.Resolve(typeof(FakeAggregate));
            act.Should().Throw<Exception>();
        }

        public ServiceProviderTypeResolverTests(ITestOutputHelper output) : base(output)
        {
            ServiceCollection.AddSingleton<IResolveType, ServiceProviderTypeResolver>();
        }
    }
}
