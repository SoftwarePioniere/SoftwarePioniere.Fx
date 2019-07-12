using System;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderClientsExtensions
    {
        public static ISopiBuilder AddClients(this ISopiBuilder builder)
        {
            var config = builder.Config;

            Console.WriteLine("Adding Auth0 Config");
            builder.Services.AddAuth0ClientOptions(config);

            Console.WriteLine("Adding AzureAd Config");
            builder.Services.AddAzureAdClientOptions(config);

            Console.WriteLine($"Fliegel 365 Auth Config Value: {builder.Options.Auth}");
            switch (builder.Options.Auth)
            {
                case SopiOptions.AuthAuth0:
                    Console.WriteLine("Adding Auth0 Token Provider");
                    builder.Services.AddAuth0TokenProvider();
                    break;
                case SopiOptions.AuthAzureAd:
                    Console.WriteLine("Adding AzureAd Token Provider");
                    builder.Services.AddAzureAdTokenProvider();
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Auth: {builder.Options.Auth}");

            }

            return builder;
        }
    }
}
