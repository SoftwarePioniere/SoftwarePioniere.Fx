using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Projections;

namespace SoftwarePioniere.Hosting
{


    public class SopiAppService : BackgroundService
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
                            try
                            {
                                await initializer.InitializeAsync(stoppingToken);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "{Type} {Inner}", initializer.GetType().FullName, GetInnerExceptionMessage(e));
                                throw;
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
                            _logger.LogError(e, "{Type} {Inner}", saga.GetType().FullName, GetInnerExceptionMessage(e));
                            throw;
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
                            _logger.LogError(e, "{Type} {Inner}", handler.GetType().FullName, GetInnerExceptionMessage(e));
                            throw;
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
                            _logger.LogError(e, "{Type}", registry.GetType().FullName);
                            throw;
                        }
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
