using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting
{
    public static class SopiBuilderConfigurationExtensions
    {
        public static ISopiBuilder AddLifetimeOptions(this ISopiBuilder builder)
        {
            return builder.AddLifetimeOptions(c => builder.Config.Bind("Lifetime", c));
        }

        public static ISopiBuilder AddLifetimeOptions(this ISopiBuilder builder,
            Action<LifetimeOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            return builder;
        }

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


        public static ISopiBuilder AddMessageBusOptions(this ISopiBuilder builder)
        {
            return builder.AddMessageBusOptions(c => builder.Config.Bind("MessageBus", c));
        }

        public static ISopiBuilder AddMessageBusOptions(this ISopiBuilder builder,
            Action<MessageBusOptions> configureOptions)
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
