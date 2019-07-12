using System;
using Microsoft.Extensions.Configuration;
using SoftwarePioniere.Extensions.Builder;

namespace SoftwarePioniere.Sample.Hosting
{
    public static class AppConfig
    {

        public static void Configure(IConfigurationBuilder builder)
        {
            builder.AddSecretsAndEnvironment("sopisample");
        }

        public static void SetEnvironmentVariables(string appId)
        {

            Environment.SetEnvironmentVariable("SOPISAMPLE_AppId", appId);
            Environment.SetEnvironmentVariable("SOPISAMPLE_Logging__UseSeq", "true");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Logging__Seq__ServerUrl", "http://localhost:5341");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Logging__WarningSources", "Microsoft");
            Environment.SetEnvironmentVariable("SOPISAMPLE_ApplicationInsights__InstrumentationKey", "2ab135ee-5f4c-4c4c-83cc-f84a036691e6");

            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth", SopiOptions.AuthAuth0);
            Environment.SetEnvironmentVariable("SOPISAMPLE_MessageBus", SopiOptions.MessageBusRedis);
            Environment.SetEnvironmentVariable("SOPISAMPLE_EntityStore", SopiOptions.EntityStoreMongoDb);
            Environment.SetEnvironmentVariable("SOPISAMPLE_DomainEventStore", SopiOptions.DomainEventStoreEventStore);
            Environment.SetEnvironmentVariable("SOPISAMPLE_CacheClient", SopiOptions.CacheRedis);
            Environment.SetEnvironmentVariable("SOPISAMPLE_Storage", SopiOptions.StorageFolder);

            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0__TenantId", "softwarepioniere-demo.eu.auth0.com");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0__Audience", "https://testapi.softwarepioniere-demo.de");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0__AdminGroupId", "975b884d-a98c-4964-90d0-d9aa3c1a0a6c");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0__SwaggerClientId", "AehgdR0ePhojFtJu6BnFGox64CzKADGW");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0__SwaggerClientSecret", "6T4firzPSdeVgpyKr21zBVr1PG1I_LyKWkhImLcNbHC_rRpV6XjcfCuESNg88nij");

            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0Client__TenantId", "softwarepioniere-demo.eu.auth0.com");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0Client__ClientId", "i8JSdWkFLF0BQHQF57pqDK5Z5647iWkM");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0Client__ClientSecret", "1s21gk950ciIydJaeUdzZMH79D7W4ccFp94vwcCvtuyQE7u4xqZdOrDp2MMYpJxj");
           
        }

        public static void SetWebSocketEnvironmentVariables(string path)
        {
            Environment.SetEnvironmentVariable("SOPISAMPLE_WebSocketPaths", path);
            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0__ContextTokenAddPaths", path);
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAd__ContextTokenAddPaths", path);
        }
    }
}
