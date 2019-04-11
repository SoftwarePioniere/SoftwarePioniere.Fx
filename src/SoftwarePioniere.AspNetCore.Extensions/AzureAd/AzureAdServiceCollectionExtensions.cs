using System;
using Microsoft.Extensions.Configuration;
using SoftwarePioniere.AspNetCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{

    public static class AzureAdServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureAdOptions(this IServiceCollection services, IConfiguration config)
        {
            return AddAzureAdOptions(services, options => config.Bind("AzureAd", options));
        }
        

        public static IServiceCollection AddAzureAdOptions(this IServiceCollection services, Action<AzureAdOptions> configureOptions)
        {
            services.AddOptions()
                .Configure(configureOptions);

            return services;
        }

    }
}
