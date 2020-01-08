using System;
using Microsoft.Extensions.Configuration;
using SoftwarePioniere.Builder;
using SoftwarePioniere.Hosting;

namespace Lib.Hosting
{
    public static class AppConfig
    {
        public const string AzureAdResource = "https://testapi.softwarepioniere-demo.de";
        public const string AzureAdAdminGroupId = "975b884d-a98c-4964-90d0-d9aa3c1a0a6c";
        public const string AzureAdTenantId = "74a8c6fa-684f-4b5a-b174-34428871d801";
        public const string AzureAdUserGroupId = "717f59a3-17e0-44a3-9c2d-fbf16e7333d7";
        public const string AzureAdSwaggerClientId = "90b324a8-eff9-4bda-a5e8-19eafc709b10";


        public static void Configure(IConfigurationBuilder builder)
        {
            builder.AddSecretsAndEnvironment("sopisample");
        }

        public static void SetEnvironmentVariables(string appId)
        {

            Environment.SetEnvironmentVariable("SOPISAMPLE_AppId", appId);
            Environment.SetEnvironmentVariable("SOPISAMPLE_AppContext", "sopifx-sample");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Logging__UseSeq", "true");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Logging__Seq__ServerUrl", "http://localhost:5341");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Logging__DebugSources", "SoftwarePioniere.Messaging");
            Environment.SetEnvironmentVariable("SOPISAMPLE_Logging__WarningSources", "Microsoft");
            Environment.SetEnvironmentVariable("SOPISAMPLE_ApplicationInsights__InstrumentationKey", "0f6120cc-905a-4a92-b53d-5a9ce070621a");

            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth", SopiOptions.AuthAzureAd);
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

           
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAd__TenantId", AzureAdTenantId);
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAd__Resource", AzureAdResource);
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAd__AdminGroupId", AzureAdAdminGroupId);
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAd__UserGroupId", AzureAdUserGroupId);
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAd__SwaggerClientId", AzureAdSwaggerClientId);
            
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAdClient__TenantId", "softwarepioniere-demo.eu.auth0.com");
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAdClient__ClientId", "90b324a8-eff9-4bda-a5e8-19eafc709b10");
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAdClient__ClientSecret", "8FOt08g97cm+BTpTI5/6aHLFw2J4hdqCHK5Unn+naU0=");

            
            //Environment.SetEnvironmentVariable("SOPISAMPLE_EventStore__TcpPort", "1193");
            //Environment.SetEnvironmentVariable("SOPISAMPLE_EventStore__HttpPort", "2193");
            Environment.SetEnvironmentVariable("SOPISAMPLE_EventStore__ExtSecureTcpPort", "1195");
            
            Environment.SetEnvironmentVariable("SOPISAMPLE_FolderStorage__Folder", "./storage");

            //Environment.SetEnvironmentVariable("SOPISAMPLE_Redis__ConnectionString", "localhost:6399,allowAdmin=true");

            //Environment.SetEnvironmentVariable("SOPISAMPLE_MongoDb__Port", "27097");

            //Environment.SetEnvironmentVariable("SOPISAMPLE_MongoDb__DatabaseId", "sopifx1");
            //Environment.SetEnvironmentVariable("SOPISAMPLE_Caching__CacheScope", "sopifx1");
        }

        public static void SetWebSocketEnvironmentVariables(string path)
        {
            Environment.SetEnvironmentVariable("SOPISAMPLE_WebSocketPaths", path);
            Environment.SetEnvironmentVariable("SOPISAMPLE_Auth0__ContextTokenAddPaths", path);
            Environment.SetEnvironmentVariable("SOPISAMPLE_AzureAd__ContextTokenAddPaths", path);
        }
    }
}
