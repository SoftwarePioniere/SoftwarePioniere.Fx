using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.DomainModel;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;

namespace SoftwarePioniere.Extensions.Hosting
{
    public class SopiAppService: BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;

        public SopiAppService(ILoggerFactory loggerFactory, IServiceProvider provider)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _provider = provider ?? throw new ArgumentNullException(nameof(provider));


            _logger = loggerFactory.CreateLogger(GetType());
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
               var sw = Stopwatch.StartNew();
            _logger.LogInformation("Starting SopiAppService");

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
                            await initializer.InitializeAsync(stoppingToken);
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
                        _logger.LogInformation("Start Saga {Saga}", saga.GetType().FullName);
                        await saga.StartAsync(stoppingToken);
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
                        _logger.LogInformation("Start MessageHandler {MessageHandler}", handler.GetType().FullName);
                        await handler.StartAsync(stoppingToken);
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
                        _logger.LogInformation("Start ProjectorRegistry {ProjectorRegistry}", registry.GetType().FullName);
                        await registry.StartAsync(stoppingToken);
                    }

                    sw1.Stop();
                    _logger.LogInformation("ProjectorRegistries Start Finished in {Elapsed:0.0000} ms", sw1.ElapsedMilliseconds);
                }
            }

            sw.Stop();
            _logger.LogInformation("SopiAppService Started in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);
        }
    }
}
