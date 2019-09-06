using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Hosting;

namespace SoftwarePioniere
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddSopiService<T>(this IServiceCollection services) where T : class, ISopiService
        {
            return services.AddSingleton<ISopiService, T>();
        }
    }
}
