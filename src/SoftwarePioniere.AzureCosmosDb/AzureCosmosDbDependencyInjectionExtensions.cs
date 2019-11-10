using System;
using SoftwarePioniere.AzureCosmosDb;
using SoftwarePioniere.Hosting;
using SoftwarePioniere.ReadModel;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureCosmosDbDependencyInjectionExtensions
    {
      
        public static IServiceCollection AddAzureCosmosDbEntityStore(this IServiceCollection services, Action<AzureCosmosDbOptions> configureOptions)
        {

            services
                .AddOptions()
                .Configure(configureOptions);

            services
                .AddSingleton<AzureComsosDbConnectionProvider3>()
                .AddSingleton<IConnectionProvider>(provider => provider.GetRequiredService<AzureComsosDbConnectionProvider3>())
                .AddSingleton<IEntityStoreConnectionProvider>(provider => provider.GetRequiredService<AzureComsosDbConnectionProvider3>())
                .AddSingleton<IEntityStore, AzureCosmodbEntityStore3>();

            return services;
        }
    }
}
