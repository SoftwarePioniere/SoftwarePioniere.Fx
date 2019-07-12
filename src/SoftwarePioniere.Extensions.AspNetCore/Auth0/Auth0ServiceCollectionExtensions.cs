using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.AspNetCore.Auth0
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