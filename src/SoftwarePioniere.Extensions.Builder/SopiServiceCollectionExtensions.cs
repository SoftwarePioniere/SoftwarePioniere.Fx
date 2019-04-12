using System;
using SoftwarePioniere.Extensions.Builder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class SopiServiceCollectionExtensions
    {
        public static ISopiBuilder AddSopi(this IServiceCollection services, Action<SopiOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            var sopiBuilder = services.AddSopi();
            sopiBuilder.Services.Configure(setupAction);

            sopiBuilder.Options = new SopiOptions();
            setupAction(sopiBuilder.Options);

            return sopiBuilder;
        }

        private static ISopiBuilder AddSopi(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return new SopiBuilder(services);
        }
    }
}