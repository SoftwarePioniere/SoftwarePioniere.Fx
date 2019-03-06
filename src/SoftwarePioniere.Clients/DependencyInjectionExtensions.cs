using System;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Clients;

// ReSharper disable once CheckNamespace
namespace SoftwarePioniere.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {

        //public static IServiceCollection AddAuth0ClientOptions(this IServiceCollection services, IConfiguration config)
        //{
        //    return services.AddAuth0ClientOptions(c => config.Bind("Auth0Client", c));
        //}

        //public static IServiceCollection AddAzureAdClientOptions(this IServiceCollection services, IConfiguration config)
        //{
        //    return services.AddAzureAdClientOptions(c => config.Bind("AzureAdClient", c));
        //}

        public static IServiceCollection AddAuth0ClientOptions(this IServiceCollection services,
            Action<Auth0ClientOptions> configureOptions)
        {
            services.Configure(configureOptions)
                .AddSingleton<Auth0TokenProvider>()
                ;

            //var settings = new Auth0ClientOptions();
            //configureOptions(settings);

            return services;
        }

        public static IServiceCollection AddAzureAdClientOptions(this IServiceCollection services,
            Action<AzureAdClientOptions> configureOptions)
        {
            services.Configure(configureOptions)
                .AddSingleton<AzureAdTokenProvider>()
                ;

            //var settings = new AzureAdClientOptions();
            //configureOptions(settings);


            return services;
        }

        public static IServiceCollection AddAuth0Client(this IServiceCollection services)
        {
            return services
                .AddSingleton<Auth0TokenProvider>()
                .AddSingleton<ITokenProvider>(c => c.GetRequiredService<Auth0TokenProvider>());
        }

        public static IServiceCollection AddAzureAdClient(this IServiceCollection services)
        {
            return services
                .AddSingleton<AzureAdTokenProvider>()
                .AddSingleton<ITokenProvider>(c => c.GetRequiredService<AzureAdTokenProvider>());
        }
    }
}
