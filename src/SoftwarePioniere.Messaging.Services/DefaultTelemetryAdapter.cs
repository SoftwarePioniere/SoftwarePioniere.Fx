using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Messaging.Services
{
    public class DefaultTelemetryAdapter : ITelemetryAdapter
    {
        private readonly ILogger _logger;

        public DefaultTelemetryAdapter(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());
        }

        
        public async Task<T> RunWithResultAsync<T>(string operationName, Func<IDictionary<string, string>, Task<T>> runx, IDictionary<string, string> parentState, ILogger logger)
        {
            if (logger == null)
                logger = _logger;

            var state = new Dictionary<string, string>();
            state.Merge(parentState);

            try
            {

                using (logger.BeginScope(state.CreateLoggerScope()))
                {
                    logger.LogInformation(operationName);

                    var sw = Stopwatch.StartNew();
                    logger.LogDebug($"{operationName} Starting");

                    var result = await runx(state);

                    sw.Stop();
                    logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);

                    return result;
                }
            }
            catch (Exception e) when (LogError(e))
            {
                throw;
            }

            bool LogError(Exception ex)
            {
                logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
                return true;
            }
        }

        public async Task RunDependencyAsync(string operationName, string type, Func<IDictionary<string, string>, Task> runx, IDictionary<string, string> parentState, ILogger logger)
        {
            if (logger == null)
                logger = _logger;

            var state = new Dictionary<string, string>();
            state.Merge(parentState);

            try
            {

                using (logger.BeginScope(state.CreateLoggerScope()))
                {
                    logger.LogInformation(operationName);

                    var sw = Stopwatch.StartNew();
                    logger.LogDebug($"{operationName} Starting");

                    await runx(state);

                    sw.Stop();
                    logger.LogInformation(operationName + " Finished in {Elapsed:0.0000} ms", sw.ElapsedMilliseconds);

                }
            }
            catch (Exception e) when (LogError(e))
            {

                throw;
            }

            bool LogError(Exception ex)
            {
                logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
                return true;
            }
        }
    }
}