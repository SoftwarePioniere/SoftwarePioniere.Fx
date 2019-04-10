using Foundatio.Caching;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Foundatio;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInMemoryCacheClient(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheClient>(new InMemoryCacheClient(new InMemoryCacheClientOptions
            {
                LoggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>()
            }));
        }

        public static IServiceCollection AddInMemoryMessageBus(this IServiceCollection services)
        {
            
            return services.AddSingleton<IMessageBus2>(p =>
                    {
                        var b = new InMemoryMessageBusOptionsBuilder()
                            .LoggerFactory(p.GetRequiredService<ILoggerFactory>());

                        return new InMemoryMessageBus2(b.Build());
                    })
                    .AddSingleton<IMessageBus>(c => c.GetRequiredService<IMessageBus2>())
                    .AddSingleton<IMessageSubscriber2>(c => c.GetRequiredService<IMessageBus2>())
                    .AddSingleton<IMessageSubscriber>(c => c.GetRequiredService<IMessageBus2>())
                    .AddSingleton<IMessagePublisher>(c => c.GetRequiredService<IMessageBus2>())
                    .AddSingleton<IQueueFactory,InMemoryQueueFactory>()
                ;
        }
    }
}
