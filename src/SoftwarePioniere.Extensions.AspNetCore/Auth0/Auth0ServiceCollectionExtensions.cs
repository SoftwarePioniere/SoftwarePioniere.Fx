using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Extensions.AspNetCore.Swagger;

namespace SoftwarePioniere.Extensions.AspNetCore.Auth0
{
    public static class Auth0ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuth0Authorization(this IServiceCollection services)
        {
            Console.WriteLine("AddAuth0Authorization");

            services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuth0AuthorizationOptions>();
            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddAuth0Options(this IServiceCollection services, IConfiguration config)
        {
            return AddAuth0Options(services, options => config.Bind("Auth0", options));
        }

        public static IServiceCollection AddAuth0Options(this IServiceCollection services, Action<Auth0Options> configureOptions)
        {
            services.AddOptions()
                .Configure(configureOptions);

            services.AddSingleton<IConfigureOptions<SopiSwaggerClientOptions>, ConfigureSopiSwaggerClientOptions>();

            return services;
        }

        public class ConfigureSopiSwaggerClientOptions : IConfigureNamedOptions<SopiSwaggerClientOptions>
        {
            private readonly Auth0Options _auth0Options;

            public ConfigureSopiSwaggerClientOptions(IOptions<Auth0Options> auth0Options)
            {
                _auth0Options = auth0Options.Value;
            }

            public void Configure(SopiSwaggerClientOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public void Configure(string name, SopiSwaggerClientOptions options)
            {
                options.AuthorizationUrl = $"{_auth0Options.Domain}authorize";
                options.ClientId = _auth0Options.SwaggerClientId;
                options.ClientSecret = _auth0Options.SwaggerClientSecret;
                options.Resource = _auth0Options.SwaggerResource;
            }
        }

        public class ConfigureAuth0AuthorizationOptions : IConfigureNamedOptions<AuthorizationOptions>
        {
            private readonly Auth0Options _auth0Options;

            public ConfigureAuth0AuthorizationOptions(IOptions<Auth0Options> auth0Options)
            {
                _auth0Options = auth0Options.Value;
            }

            public void Configure(AuthorizationOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public void Configure(string name, AuthorizationOptions options)
            {
                if (!string.IsNullOrEmpty(_auth0Options.AdminGroupId))
                    options.AddPolicy(PolicyConstants.IsAdminPolicy,
                        policy => policy.RequireClaim(_auth0Options.GroupClaimType, _auth0Options.AdminGroupId));

                if (!string.IsNullOrEmpty(_auth0Options.UserGroupId))
                    options.AddPolicy(PolicyConstants.IsUserPolicy,
                        policy => policy.RequireClaim(_auth0Options.GroupClaimType, _auth0Options.UserGroupId));
            }
        }
    }
}