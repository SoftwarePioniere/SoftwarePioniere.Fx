using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SoftwarePioniere.Extensions.AspNetCore.Swagger
{
    public static class SopiSwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddSopiSwaggerForMultipleServices(this IServiceCollection services, string titleName, string baseRoute, string serviceName, string[] apiKeys, bool readOnly
        , Action<SopiSwaggerOptions> configureOptions = null)
        {
            Console.WriteLine("AddSopiSwaggerForMultipleServices");

            services.AddSingleton<IConfigureOptions<SopiSwaggerOptions>>(p =>
                new ConfigureSopiSwaggerOptionsForMultipleServices(
                    p.GetRequiredService<IOptions<SopiSwaggerClientOptions>>(),
                    titleName, baseRoute, serviceName, apiKeys, readOnly, configureOptions
                    ));

            services
                .AddSopiSwagger()
                .AddSwaggerGen();

            return services;
        }

        private class ConfigureSopiSwaggerOptionsForMultipleServices : IConfigureOptions<SopiSwaggerOptions>
        {
            private readonly string _titleName;
            private readonly string _baseRoute;
            private readonly string _serviceName;
            private readonly string[] _apiKeys;
            private readonly bool _readOnly;
            private readonly Action<SopiSwaggerOptions> _configureOptions;
            private readonly SopiSwaggerClientOptions _swaggerClientOptions;

            public ConfigureSopiSwaggerOptionsForMultipleServices(
                IOptions<SopiSwaggerClientOptions> swaggerClientOptions,
                string titleName, string baseRoute, string serviceName, string[] apiKeys, bool readOnly
                , Action<SopiSwaggerOptions> configureOptions = null)
            {
                _swaggerClientOptions = swaggerClientOptions.Value;
                _titleName = titleName;
                _baseRoute = baseRoute;
                _serviceName = serviceName;
                _apiKeys = apiKeys;
                _readOnly = readOnly;
                _configureOptions = configureOptions;
            }
            public void Configure(SopiSwaggerOptions c)
            {
                c.Title = $"{_titleName} {_serviceName}";
                c.Doc = _apiKeys[0];

                var docs = _apiKeys.Select(apiKey => new SwaggerDocOptions { Name = apiKey, Title = c.Title, Url = $"/{_baseRoute}/{_serviceName}/swagger/{apiKey}.json" }).ToArray();
                c.Docs = docs;
                c.RouteTemplate = $"{_baseRoute}/{_serviceName}" + "/swagger/{documentName}.json";
                c.ReadOnlyUi = _readOnly;

                if (!_readOnly)
                {
                    var scopes = new Dictionary<string, string>();
                    //{
                    //    {"admin", "admin access"}
                    //};

                    c.OAuth2Scheme = new OAuth2Scheme
                    {
                        Type = "oauth2",
                        Flow = "implicit",
                        AuthorizationUrl = _swaggerClientOptions.AuthorizationUrl,
                        Scopes = scopes
                    };

                    c.OAuthAdditionalQueryStringParams = new Dictionary<string, string>
                {
                    {"resource", _swaggerClientOptions.Resource},
                    {"Audience", _swaggerClientOptions.Resource}
                };

                    c.OAuthClientId = _swaggerClientOptions.ClientId;
                    c.OAuthClientSecret = _swaggerClientOptions.ClientSecret;
                }
                _configureOptions?.Invoke(c);
            }
        }

        public static IServiceCollection AddSopiSwaggerForSingleService(this IServiceCollection services, string apiKey, string baseRoute, string serviceName, bool readOnly
        , Action<SopiSwaggerOptions> configureOptions = null)
        {
            Console.WriteLine("AddSopiSwaggerForSingleService");


            services.AddSingleton<IConfigureOptions<SopiSwaggerOptions>>(p =>
                new ConfigureSopiSwaggerForSingleService(
                    p.GetRequiredService<IOptions<SopiSwaggerClientOptions>>(),
                    apiKey, baseRoute, serviceName, readOnly, configureOptions
                ));

            return services;
        }

        private class ConfigureSopiSwaggerForSingleService : IConfigureOptions<SopiSwaggerOptions>
        {
            private readonly string _apiKey;
            private readonly string _baseRoute;
            private readonly string _serviceName;
            private readonly bool _readOnly;
            private readonly Action<SopiSwaggerOptions> _configureOptions;
            private readonly SopiSwaggerClientOptions _swaggerClientOptions;

            public ConfigureSopiSwaggerForSingleService(
                IOptions<SopiSwaggerClientOptions> swaggerClientOptions,
                string apiKey, string baseRoute, string serviceName, bool readOnly
                , Action<SopiSwaggerOptions> configureOptions = null)
            {
                _swaggerClientOptions = swaggerClientOptions.Value;

                _apiKey = apiKey;
                _baseRoute = baseRoute;
                _serviceName = serviceName;
                _readOnly = readOnly;
                _configureOptions = configureOptions;
            }
            public void Configure(SopiSwaggerOptions c)
            {
                c.Title = $"{_apiKey} {_serviceName}";
                c.Doc = _apiKey;
                c.Docs = new[]
                {
                    new SwaggerDocOptions{Name = _apiKey , Title  = c.Title,  Url=$"/{_baseRoute}/{_serviceName}/swagger/{_apiKey}.json" }
                };
                c.RouteTemplate = $"{_baseRoute}/{_serviceName}" + "/swagger/{documentName}.json";
                c.ServiceName = _serviceName;
                c.ReadOnlyUi = _readOnly;

                if (!_readOnly)
                {

                    var scopes = new Dictionary<string, string>();
                    //{
                    //    {"admin", "admin access"}
                    //};

                    c.OAuth2Scheme = new OAuth2Scheme
                    {
                        Type = "oauth2",
                        Flow = "implicit",
                        AuthorizationUrl = _swaggerClientOptions.AuthorizationUrl,
                        Scopes = scopes
                    };

                    c.OAuthAdditionalQueryStringParams = new Dictionary<string, string>
                    {
                        {"resource", _swaggerClientOptions.Resource},
                        {"Audience", _swaggerClientOptions.Resource}
                    };

                    c.OAuthClientId = _swaggerClientOptions.ClientId;
                    c.OAuthClientSecret = _swaggerClientOptions.ClientSecret;
                }
                _configureOptions?.Invoke(c);
            }


        }

        public static IServiceCollection AddSopiSwagger(this IServiceCollection services, Action<SopiSwaggerOptions> configureOptions = null)
        {
            Console.WriteLine("AddSopiSwagger");

            if (configureOptions != null)
            {
                services.AddOptions()
                    .Configure(configureOptions);
            }

            //  services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGen>();

            services.AddOptions<SwaggerGenOptions>()
                .Configure<IOptions<SopiSwaggerOptions>>((options, sopiSwaggerOptionsT) =>
                {
                    var sopiSwaggerOptions = sopiSwaggerOptionsT.Value;
                    if (sopiSwaggerOptions.Docs != null)
                    {
                        foreach (var doc in sopiSwaggerOptions.Docs)
                        {
                            options.SwaggerDoc(doc.Name, new Info
                            {
                                Title = doc.Title,
                                Version = "v1"
                            });
                        }
                    }
                    else
                    {
                        var doc = "api";

                        if (!string.IsNullOrEmpty(sopiSwaggerOptions.Doc))
                        {
                            doc = sopiSwaggerOptions.Doc;
                        }

                        options.SwaggerDoc(doc, new Info
                        {
                            Title = $"{sopiSwaggerOptions.Title}-{doc}",
                            Version = "v1"
                        });
                    }

                    if (sopiSwaggerOptions.XmlFiles != null)
                    {
                        foreach (var xmlFile in sopiSwaggerOptions.XmlFiles)
                        {
                            IncludeXmlCommentsIfExist(options, xmlFile);
                        }
                    }

                    options.EnableAnnotations();
                    options.DescribeAllEnumsAsStrings();
                    options.OperationFilter<SummaryFromOperationFilter>();
                    options.OperationFilter<FormFileOperationFilter>();
                    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                    

                    if (!sopiSwaggerOptions.ReadOnlyUi)
                    {

                        if (!string.IsNullOrEmpty(sopiSwaggerOptions.AuthorizationUrl))
                        {
                            var sch = new OAuth2Scheme
                            {
                                Type = "oauth2",
                                Flow = "implicit",
                                AuthorizationUrl = sopiSwaggerOptions.AuthorizationUrl,
                                Scopes = sopiSwaggerOptions.Scopes
                            };
                            options.AddSecurityDefinition("oauth2", sch);
                        }

                        options.AddSecurityDefinition("oauth2", sopiSwaggerOptions.OAuth2Scheme);
                    }
                    
                    options.OperationFilter<SecurityRequirementsOperationFilter>();

                    options.DocInclusionPredicate((s, description) =>
                    {
                        if (sopiSwaggerOptions.Docs == null) return true;

                        if (sopiSwaggerOptions.Docs.Select(x => x.Name).Contains(s))
                            return description.GroupName == s;

                        if (string.IsNullOrEmpty(description.GroupName))
                            return true;

                        return description.GroupName != s;
                    });

                });

            services.AddSwaggerGen();

            return services;
        }

        public class ConfigureSwaggerGen : IConfigureNamedOptions<SwaggerGenOptions>
        {
            private readonly SopiSwaggerOptions _sopiSwaggerOptions;

            public ConfigureSwaggerGen(IOptions<SopiSwaggerOptions> sopiSwaggerOptions)
            {
                _sopiSwaggerOptions = sopiSwaggerOptions.Value;
            }
            public void Configure(SwaggerGenOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public void Configure(string name, SwaggerGenOptions options)
            {
                if (_sopiSwaggerOptions.Docs != null)
                {
                    foreach (var doc in _sopiSwaggerOptions.Docs)
                    {
                        options.SwaggerDoc(doc.Name, new Info
                        {
                            Title = doc.Title,
                            Version = "v1"
                        });
                    }
                }
                else
                {
                    var doc = "api";

                    if (!string.IsNullOrEmpty(_sopiSwaggerOptions.Doc))
                    {
                        doc = _sopiSwaggerOptions.Doc;
                    }

                    options.SwaggerDoc(doc, new Info
                    {
                        Title = $"{_sopiSwaggerOptions.Title}-{doc}",
                        Version = "v1"
                    });
                }

                if (_sopiSwaggerOptions.XmlFiles != null)
                {
                    foreach (var xmlFile in _sopiSwaggerOptions.XmlFiles)
                    {
                        IncludeXmlCommentsIfExist(options, xmlFile);
                    }
                }

                options.EnableAnnotations();
                options.DescribeAllEnumsAsStrings();
                options.OperationFilter<FormFileOperationFilter>();
                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                options.OperationFilter<SummaryFromOperationFilter>();

                if (!string.IsNullOrEmpty(_sopiSwaggerOptions.AuthorizationUrl))
                {
                    var sch = new OAuth2Scheme
                    {
                        Type = "oauth2",
                        Flow = "implicit",
                        AuthorizationUrl = _sopiSwaggerOptions.AuthorizationUrl,
                        Scopes = _sopiSwaggerOptions.Scopes
                    };
                    options.AddSecurityDefinition("oauth2", sch);
                }

                options.AddSecurityDefinition("oauth2", _sopiSwaggerOptions.OAuth2Scheme);

                options.OperationFilter<SecurityRequirementsOperationFilter>();

                options.DocInclusionPredicate((s, description) =>
                {
                    if (_sopiSwaggerOptions.Docs == null) return true;

                    if (_sopiSwaggerOptions.Docs.Select(x => x.Name).Contains(s))
                        return description.GroupName == s;

                    if (string.IsNullOrEmpty(description.GroupName))
                        return true;

                    return description.GroupName != s;
                });
            }
        }

        private static void IncludeXmlCommentsIfExist(this SwaggerGenOptions swaggerGenOptions, string fileName)
        {
            //var xmlFileName = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, fileName);
            var xmlFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            //var xmlFileName = fileName;
            var exist = File.Exists(xmlFileName);

            if (exist)
            {
                swaggerGenOptions.IncludeXmlComments(xmlFileName);
            }
        }
    }
}