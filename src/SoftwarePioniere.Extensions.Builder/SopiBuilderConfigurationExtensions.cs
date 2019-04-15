using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwarePioniere.Extensions.Builder
{
    public static class SopiBuilderConfigurationExtensions
    {
        public static ISopiBuilder AddDevOptions(this ISopiBuilder builder)
        {
            return builder.AddDevOptions(c => builder.Config.Bind("DevOptions", c));
        }

        public static ISopiBuilder AddDevOptions(this ISopiBuilder builder,
            Action<DevOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            return builder;
        }

        public static ISopiBuilder AddReportingOptions(this ISopiBuilder builder)
        {
            return builder.AddReportingOptions(c => builder.Config.Bind("Reporting", c));
        }

        public static ISopiBuilder AddReportingOptions(this ISopiBuilder builder,
            Action<ReportingOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            return builder;
        }


        public static ISopiBuilder AddConfiguration(this ISopiBuilder builder, IConfiguration config)
        {
            builder.Config = config;

            builder.Services.Configure<SopiOptions>(config.Bind);

            builder.Options = new SopiOptions();
            config.Bind(builder.Options);

            return builder;
        }
    }
}
