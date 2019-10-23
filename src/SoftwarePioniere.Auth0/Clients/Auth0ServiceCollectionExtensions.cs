using System;
using Microsoft.Extensions.Configuration;
using SoftwarePioniere.Auth0.Clients;
using SoftwarePioniere.Clients;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class Auth0ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuth0ClientOptions(this IServiceCollection services, IConfiguration config)
        {
            return services.AddAuth0ClientOptions(c => config.Bind("Auth0Client", c));
        }

        public static IServiceCollection AddAuth0ClientOptions(this IServiceCollection services,
            Action<Auth0ClientOptions> configureOptions)
        {
            services.Configure(configureOptions)
                .AddSingleton<Auth0TokenProvider>()
                ;

            return services;
        }

        public static IServiceCollection AddAuth0TokenProvider(this IServiceCollection services)
        {
            return services
                .AddSingleton<Auth0TokenProvider>()
                .AddSingleton<ITokenProvider>(c => c.GetRequiredService<Auth0TokenProvider>());
        }

    }
}
