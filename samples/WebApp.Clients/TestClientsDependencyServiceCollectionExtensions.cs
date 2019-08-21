using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftwarePioniere.Clients;

// ReSharper disable once CheckNamespace
namespace WebApp.Clients
{
    public static class TestClientsDependencyServiceCollectionExtensions
    {
        public static IServiceCollection AddTestClientOptions(this IServiceCollection services,
          IConfiguration config)
        {
            return services.AddTestClientOptions(c => config.Bind("TestClient", c));
        }

        public static IServiceCollection AddTestClientOptions(this IServiceCollection services,
            Action<ClientOptions> configureOptions)
        {

            services
                .AddOptions<ClientOptions>()
                    .Configure(configureOptions);

            return services;
        }


        public static IServiceCollection AddTestSystemClient(this IServiceCollection services)
        {

            services
                .AddHttpClient("TestSystemClient", (provider, client) =>
                {
                    var opt = provider.GetRequiredService<IOptions<ClientOptions>>();
                    client.BaseAddress = new Uri(opt.Value.BaseAddress);
                })
                .AddHttpMessageHandler(provider =>
                    new AccessTokenHandler(provider.GetService<ILoggerFactory>(), provider.GetService<ITokenProvider>(), "https://testapi.softwarepioniere-demo.de"));

            services.AddTransient<ITestSystemClient>(provider =>
                new TestSystemClient(provider.GetRequiredService<ILoggerFactory>(),
                    provider.GetRequiredService<IHttpClientFactory>().CreateClient("TestSystemClient")
                ));


            return services;
        }


        public static IServiceCollection AddTestUserClient(this IServiceCollection services)
        {

            services.AddHttpClient("TestUserClient", (provider, client) =>
            {
                var opt = provider.GetRequiredService<IOptions<ClientOptions>>();
                client.BaseAddress = new Uri(opt.Value.BaseAddress);
            });

            services.AddTransient<ITestUserClient>(provider =>
                new TestUserClient(provider.GetRequiredService<ILoggerFactory>(),
                    provider.GetRequiredService<IHttpClientFactory>().CreateClient("TestUserClient")
                ));


            return services;
        }

    }
}
