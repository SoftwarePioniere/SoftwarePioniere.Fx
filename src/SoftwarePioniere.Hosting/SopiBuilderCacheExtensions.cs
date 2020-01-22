using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderCacheExtensions
    {
        //private static string CreateDatabaseId(ISopiBuilder builder)
        //{
        //    var options = new CacheOptions();
        //    builder.Config.Bind("Caching", options);

        //    if (!string.IsNullOrEmpty(options.CacheScope))
        //        return options.CacheScope;

        //    return builder.Options.CreateDatabaseId();
        //}

        public static ISopiBuilder AddCacheClient(this ISopiBuilder builder)
        {
            var config = builder.Config;

            builder.Services.AddCachingOptions(c => config.Bind("Caching"));


            builder.Log($"Sopi CacheClient Config Value: {builder.Options.CacheClient}");
            switch (builder.Options.CacheClient)
            {
                case SopiOptions.CacheRedis:
                    builder.Log("Adding Redis CacheClient");
                    builder.Services.AddRedisCacheClient();
                    break;

                default:
                    builder.Log("Adding InMemory CacheClient");
                    builder.AddInMemoryCacheClient();
                    break;
            }

            return builder;
        }

        public static ISopiBuilder AddInMemoryCacheClient(this ISopiBuilder builder)
        {
            var services = builder.Services;
            services.AddInMemoryCacheClient();

            builder.Services.PostConfigure<SopiOptions>(c =>
            {
                c.CacheClient = SopiOptions.InMemory;
            });

            return builder;
        }
    }
}
