using System;
using Microsoft.Extensions.Configuration;
using SoftwarePioniere.Clients;
using SoftwarePioniere.Clients.AzureAd;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureAdServiceCollectionExtensions
    {

        public static IServiceCollection AddAzureAdClientOptions(this IServiceCollection services, IConfiguration config)
        {
            return services.AddAzureAdClientOptions(c => config.Bind("AzureAdClient", c));
        }
        

        public static IServiceCollection AddAzureAdClientOptions(this IServiceCollection services,
            Action<AzureAdClientOptions> configureOptions)
        {
            services.Configure(configureOptions)
                .AddSingleton<AzureAdTokenProvider>()
                ;

            var settings = new AzureAdClientOptions();
            configureOptions(settings);


            return services;
        }
        
        public static IServiceCollection AddAzureAdTokenProvider(this IServiceCollection services)
        {
            return services
                .AddSingleton<AzureAdTokenProvider>()
                .AddSingleton<ITokenProvider>(c => c.GetRequiredService<AzureAdTokenProvider>());
        }
    }
}
