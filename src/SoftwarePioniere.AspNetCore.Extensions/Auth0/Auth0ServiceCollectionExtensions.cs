using System;
using Microsoft.Extensions.Configuration;
using SoftwarePioniere.AspNetCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class Auth0ServiceCollectionExtensions
    {

        public static IServiceCollection AddAuth0Options(this IServiceCollection services, IConfiguration config)
        {
            return AddAuth0Options(services, options => config.Bind("Auth0", options));
        }

        public static IServiceCollection AddAuth0Options(this IServiceCollection services, Action<Auth0Options> configureOptions)
        {
            services.AddOptions()
                .Configure(configureOptions);

            return services;
        }
    }
}