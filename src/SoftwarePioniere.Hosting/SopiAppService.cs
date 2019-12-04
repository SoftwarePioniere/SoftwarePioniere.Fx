using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Hosting
{
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SopiAppService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly SopiApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;

        public SopiAppService(ILoggerFactory loggerFactory, IServiceProvider provider, SopiApplicationLifetime applicationLifetime)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));


            _logger = loggerFactory.CreateLogger(GetType());
        }


        public static string GetInnerExceptionMessage(Exception ex)
        {
            if (ex.InnerException != null)
            {
                return GetInnerExceptionMessage(ex.InnerException);
            }

            return ex.Message;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _applicationLifetime.IsStarting = true;

            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Starting SopiAppService");

            //connection provider initializier
            {
                {

                    var initializers = _provider.GetServices<IConnectionProvider>().ToList();
                    if (initializers.Count > 0)
                    {
                        _logger.LogInformation("Starting ConnectionProvider Initialization");
                        var sw1 = Stopwatch.StartNew();
                        var done = new List<Type>();

                        foreach (var initializer in initializers)
                        {
                            if (!done.Contains(initializer.GetType()))
                            {

                                _logger.LogInformation("Initialize IConnectionProvider {ConnectionProvider}", initializer.GetType().FullName);
                                try
                                {
                                    await Policy
                                        .Handle<Exception>()
                                        .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 0.5))
                                        .ExecuteAsync(() => initializer.InitializeAsync(stoppingToken));
                                }
                                catch (Exception e)
                                {
                                    _logger.LogCritical(e, "{Type} {Inner}", initializer.GetType().FullName, GetInnerExceptionMessage(e));
                                }
                                done.Add(initializer.GetType());
                            }
                        }
                        sw1.Stop();
                        _logger.LogInformation("ConnectionProvider Initialization Finished in {Elapsed:0.0000} ms", sw1.ElapsedMilliseconds);
                    }
                }
            }

            //entitystore initializer
            {

                var initializers = _provider.GetServices<IEntityStoreInitializer>().ToList();
                if (initializers.Count > 0)
                {
                    _logger.LogInformation("Starting EntityStore Initialization");
                    var sw1 = Stopwatch.StartNew();
                    var done = new List<Type>();

                    foreach (var initializer in initializers.OrderBy(x => x.ExecutionOrder))
                    {
                        if (!done.Contains(initializer.GetType()))
                        {

                            _logger.LogInformation("Initialize IEntityStoreInitializer {EntityStoreInitializer}", initializer.GetType().FullName);
                            try
                            {
                                await Policy
                                    .Handle<Exception>()
                                    .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 0.5))
                                    .ExecuteAsync(() => initializer.InitializeAsync(stoppingToken));
                            }
                            catch (Exception e)
                            {
                                _logger.LogCritical(e, "{Type} {Inner}", initializer.GetType().FullName, GetInnerExceptionMessage(e));
                            }
                            done.Add(initializer.GetType());
                        }
                    }
                    sw1.Stop();
                    _logger.LogInformation("EntityStore Initialization Finished in {Elapsed:0.0000} ms", sw1.ElapsedMilliseconds);
                }
            }

            //eventstore initializer
            {

                var initializers = _provider.GetServices<IEventStoreInitializer>().ToList();
                if (initializers.Count > 0)
                {
                    _logger.LogInformation("Starting EventStore Initialization");
                    var sw1 = Stopwatch.StartNew();
                    var done = new List<Type>();

                    foreach (var initializer in initializers.OrderBy(x => x.ExecutionOrder))
                    {
                        if (!done.Contains(initializer.GetType()))
                        {

                            _logger.LogInformation("Initialize IEventStoreInitializer {EventStoreInitializer}", initializer.GetType().FullName);
                            try
                            {
                                await Policy
                                    .Handle<Exception>()
                                    .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 0.5))
                                    .ExecuteAsync(() => initializer.InitializeAsync(stoppingToken));
                            }
                            catch (Exception e)
                            {
                                _logger.LogCritical(e, "{Type} {Inner}", initializer.GetType().FullName, GetInnerExceptionMessage(e));
                            }
                            done.Add(initializer.GetType());
                        }
                    }
                    sw1.Stop();
                    _logger.LogInformation("EventStore Initialization Finished in {Elapsed:0.0000} ms", sw1.ElapsedMilliseconds);
                }
            }


            //saga2
            {

                var sagas = _provider.GetServices<ISaga>().ToList();
                if (sagas.Count > 0)
                {
                    _logger.LogInformation("Starting Sagas");
                    var sw1 = Stopwatch.StartNew();
                    foreach (var saga in sagas)
                    {
                        _logger.LogInformation("Start Saga {Saga}", saga.GetType().Name);
                        try
                        {
                            await saga.StartAsync(stoppingToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogCritical(e, "{Type} {Inner}", saga.GetType().FullName, GetInnerExceptionMessage(e));
                        }
                    }

                    sw1.Stop();
                    _logger.LogInformation("Saga Start Finished in {Elapsed:0.0000} ms", sw1.ElapsedMilliseconds);
                }
            }

            //message handler
            {

                var handlers = _provider.GetServices<IMessageHandler>().ToList();
                if (handlers.Count > 0)
                {
                    _logger.LogInformation("Starting MessageHandler");
                    var sw1 = Stopwatch.StartNew();
                    foreach (var handler in handlers)
                    {
                        _logger.LogInformation("Start MessageHandler {MessageHandler}", handler.GetType().Name);
                        try
                        {
                            await handler.StartAsync(stoppingToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogCritical(e, "{Type} {Inner}", handler.GetType().FullName, GetInnerExceptionMessage(e));
                        }
                    }

                    sw1.Stop();
                    _logger.LogInformation("MessageHandler Start Finished in {Elapsed:0.0000} ms", sw1.ElapsedMilliseconds);
                }
            }


            //registries handler
            {

                var registries = _provider.GetServices<IProjectorRegistry>().ToList();
                if (registries.Count > 0)
                {
                    _logger.LogInformation("Starting ProjectorRegistries");
                    var sw1 = Stopwatch.StartNew();
                    foreach (var registry in registries)
                    {
                        _logger.LogInformation("Start ProjectorRegistry {ProjectorRegistry}", registry.GetType().Name);
                        try
                        {
                            await registry.StartAsync(stoppingToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogCritical(e, "{Type}", registry.GetType().FullName);
                        }
                    }

                    sw1.Stop();
                    _logger.LogInformation("ProjectorRegistries Start Finished in {Elapsed:0.0000} ms", sw1.ElapsedMilliseconds);
                }
            }

            
            //sopi services
            {

                var services = _provider.GetServices<ISopiService>().ToList();
                if (services.Count > 0)
                {
                    _logger.LogInformation("Starting SopiServices");
                    var sw1 = Stopwatch.StartNew();
                    foreach (var service in services)
                    {
                        _logger.LogInformation("Start SopiService {SopiService}", service.GetType().Name);
                        try
                        {
                            await service.StartAsync(stoppingToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogCritical(e, "{Type}", service.GetType().FullName);
                        }
                    }

                    sw1.Stop();
                    _logger.LogInformation("SopiServices Start Finished in {Elapsed:0.0000} ms", sw1.ElapsedMilliseconds);
                }
            }

            sw.Stop();
            _logger.LogInformation("SopiAppService Started in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);

            _applicationLifetime.IsStarting = false;
            _applicationLifetime.IsStarted = true;
        }
    }
}
