using System;
using Microsoft.Extensions.Configuration;
using SoftwarePioniere.Extensions.Builder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class SopiServiceCollectionExtensions
    {
        public static ISopiBuilder AddSopi(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            //if (setupAction == null)
            //{
            //    throw new ArgumentNullException(nameof(setupAction));
            //}

            var sopiBuilder = services.AddSopi();
            sopiBuilder.AddConfiguration(config);


            return sopiBuilder;
        }

        private static ISopiBuilder AddSopi(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .Configure<DevOptions>(c =>
                {
                    c.BadRequestForPost = false;
                    c.RaiseCommandFailed = false;
                });

            return new SopiBuilder(services);
        }
    }
}