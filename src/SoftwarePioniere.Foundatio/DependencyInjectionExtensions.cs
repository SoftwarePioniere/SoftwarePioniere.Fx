using Foundatio.Caching;
using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Foundatio;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere.Extensions.DependencyInjection
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
            return services.AddSingleton<IMessageBus>(p =>
                    {
                        var b = new InMemoryMessageBusOptionsBuilder()
                            .LoggerFactory(p.GetRequiredService<ILoggerFactory>());

                        return new InMemoryMessageBus(b.Build());
                    })
                    .AddSingleton<IMessageSubscriber>(c => c.GetRequiredService<IMessageBus>())
                    .AddSingleton<IMessagePublisher>(c => c.GetRequiredService<IMessageBus>())
                  .AddSingleton<IQueueFactory,InMemoryQueueFactory>()
                ;
        }
    }
}
