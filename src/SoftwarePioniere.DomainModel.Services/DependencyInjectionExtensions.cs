using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInMemoryMessageBus(this IServiceCollection services)
        {
            return services.AddSingleton<IMessageBus>(p =>
           {
               var b = new InMemoryMessageBusOptionsBuilder()
                   .LoggerFactory(p.GetRequiredService<ILoggerFactory>());

               return new InMemoryMessageBus(b.Build());
           });
        }
    }
}
