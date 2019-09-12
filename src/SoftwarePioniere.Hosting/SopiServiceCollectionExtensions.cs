using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Builder;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere.Hosting
{
    public static class SopiServiceCollectionExtensions
    {
        public static ISopiBuilder AddSopi(this IServiceCollection services, IConfiguration config, Action<string> log = null)
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

            var sopiBuilder = services.AddSopi(log);
            sopiBuilder.AddConfiguration(config);


            return sopiBuilder;
        }

        private static ISopiBuilder AddSopi(this IServiceCollection services, Action<string> log = null)
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

            return new SopiBuilder(services, log);
        }
    }
}