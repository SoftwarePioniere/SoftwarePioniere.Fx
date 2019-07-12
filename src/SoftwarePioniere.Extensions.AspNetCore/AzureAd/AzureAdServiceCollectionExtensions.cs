using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.AspNetCore.AzureAd
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
