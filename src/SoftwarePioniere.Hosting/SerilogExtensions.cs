using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SoftwarePioniere.Builder;

namespace SoftwarePioniere.Hosting
{
    public static class SerilogExtensions
    {
        public static void ConfigureSerilog(this LoggerConfiguration loggerConfiguration, IConfiguration config,
            Action<LoggerConfiguration> setupAction = null)
        {
            Console.WriteLine("ConfigureSerilog");

            var sopiOptions = config.CreateSopiOptions();

            var options = config.CreateLoggingOptions();

            Console.WriteLine($"ConfigureSerilog:: AppVersion: {sopiOptions.AppVersion}");

            if (!Directory.Exists(options.LogDir))
            {
                Console.WriteLine($"ConfigureSerilog:: Creating LogDir: {options.LogDir}");
                Directory.CreateDirectory(options.LogDir);
            }

            if (!Enum.TryParse(options.MinimumLevel, out LogEventLevel minLevel))
            {
                minLevel = LogEventLevel.Information;
            }

            Console.WriteLine($"ConfigureSerilog:: MinimumLevel: {minLevel}");

            var logFile = Path.Combine(options.LogDir, "log.txt");
            Console.WriteLine($"ConfigureSerilog:: LogFile: {logFile}");

            loggerConfiguration
                .MinimumLevel.Is(minLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Startup", LogEventLevel.Information)
                .MinimumLevel.Override("SoftwarePioniere.Hosting", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("AppId", sopiOptions.AppId)
                .Enrich.WithProperty("AppVersion", sopiOptions.AppVersion)
                .WriteTo.File(logFile,
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: options.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    outputTemplate: options.Template)
                ;

            if (!string.IsNullOrEmpty(sopiOptions.AppContext))
            {
                loggerConfiguration.Enrich.WithProperty("AppContext", sopiOptions.AppContext);
            }

            if (
                options.DisableConsole ||
                !string.Equals(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"), Environments.Production)
                && !string.Equals(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"), Environments.Staging)
            )
            {
                loggerConfiguration.WriteTo.LiterateConsole(outputTemplate: options.Template);
            }

            if (options.UseSeq)
            {
                Console.WriteLine("ConfigureSerilog:: Adding Seq");
                loggerConfiguration
                    .WriteTo.Seq(options.SeqServerUrl);
            }

            var warningSources = options.WarningSources;
            if (!string.IsNullOrEmpty(warningSources))
            {
                Console.WriteLine("ConfigureSerilog:: Adding WarningSources");
                foreach (var source in warningSources.Split(';'))
                {
                    Console.WriteLine($"ConfigureSerilog:: Adding WarningSource {source}");
                    loggerConfiguration.MinimumLevel.Override(source, LogEventLevel.Warning);
                }
            }

            var infoSources = options.InformationSources;
            if (!string.IsNullOrEmpty(infoSources))
            {
                Console.WriteLine("ConfigureSerilog:: Adding InformationSources");
                foreach (var source in infoSources.Split(';'))
                {
                    Console.WriteLine($"ConfigureSerilog:: Adding InformationSource {source}");
                    loggerConfiguration.MinimumLevel.Override(source, LogEventLevel.Information);
                }
            }

            var debugSources = options.DebugSources;
            if (!string.IsNullOrEmpty(debugSources))
            {
                Console.WriteLine("ConfigureSerilog:: Adding DebugSources");
                foreach (var source in debugSources.Split(';'))
                {
                    Console.WriteLine($"ConfigureSerilog:: Adding DebugSource {source}");
                    loggerConfiguration.MinimumLevel.Override(source, LogEventLevel.Debug);
                }
            }

            var traceSources = options.TraceSources;
            if (!string.IsNullOrEmpty(traceSources))
            {
                Console.WriteLine("ConfigureSerilog:: Adding TraceSources");
                foreach (var source in traceSources.Split(';'))
                {
                    Console.WriteLine($"ConfigureSerilog:: Adding TraceSource {source}");
                    loggerConfiguration.MinimumLevel.Override(source, LogEventLevel.Verbose);
                }
            }

            setupAction?.Invoke(loggerConfiguration);
        }

        public static ILogger CreateSerilogger(this IConfiguration config, Action<LoggerConfiguration> setupAction = null, string sourceContext = "Startup")
        {
            Console.WriteLine("CreateSerilogger");
            var loggerConfig = new LoggerConfiguration();
            loggerConfig.ConfigureSerilog(config, setupAction);
            var logger = loggerConfig.CreateLogger()
                .ForContext(Constants.SourceContextPropertyName, sourceContext);
            Log.Logger = logger;
            return logger;
        }
    }
}