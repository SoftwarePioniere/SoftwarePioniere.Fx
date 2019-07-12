using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessagingServicesDependencyInjectionExtensions
    {
        public static IServiceCollection AddDefaultMessageBusAdapter(this IServiceCollection services)
        {
            return services.AddSingleton<IMessageBusAdapter, DefaultMessageBusAdapter>();
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
                    .AddSingleton<IQueueFactory, InMemoryQueueFactory>()
                ;
        }

    }
}
