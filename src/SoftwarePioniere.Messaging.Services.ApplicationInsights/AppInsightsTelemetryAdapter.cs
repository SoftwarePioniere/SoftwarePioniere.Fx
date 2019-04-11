using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.AspNetCore;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Messaging.Services.ApplicationInsights
{
    public class AppInsightsTelemetryAdapter : IControllerTelemetryAdapter
    {
    
        private readonly ILogger _logger;
        
        public AppInsightsTelemetryAdapter(ILoggerFactory loggerFactory
            , TelemetryClient telemetryClient        
        )
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
        
            TelemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));

            _logger = loggerFactory.CreateLogger(GetType());

         
        }

        public IDictionary<string, string> CreateState(HttpContext httpContext)
        {
            return  httpContext.CreateState();
        }


        public TelemetryClient TelemetryClient { get; }
   

        public async Task<ActionResult<T>> RunWithActionResultAsync<T>(string operationName,
            Func<IDictionary<string, string>, Task<T>> runx, IDictionary<string, string> parentState, ILogger logger)
        {
            if (logger == null)
                logger = _logger;
                                       
            var state = new Dictionary<string, string>();
            state.Merge(parentState);

            var operationTelemetry = CreateRequestOperation(operationName, state);

            try
            {
                state.SetParentRequestId(operationTelemetry.Telemetry.Id);

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
            catch (AuthorizationException e) when (LogError(e))
            {
                operationTelemetry.Telemetry.Success = false;
                TelemetryClient.TrackException(e);
                return new ForbidResult();
            }
            catch (Exception e) when (LogError(e))
            {
                operationTelemetry.Telemetry.Success = false;
                TelemetryClient.TrackException(e);
                return new BadRequestObjectResult(e.Message);
            }
            finally
            {
                TelemetryClient.StopOperation(operationTelemetry);
            }

            bool LogError(Exception ex)
            {
                logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
                return true;
            }
        }

        public Task<ActionResult<T>> RunWithActionResultAsync<T>(string operationName, Func<IDictionary<string, string>, Task<T>> runx, HttpContext httpContext, ILogger logger)
        {
            return RunWithActionResultAsync(operationName, runx, CreateState(httpContext), logger);
        }

        public async Task<T> RunWithResultAsync<T>(string operationName, Func<IDictionary<string, string>, Task<T>> runx, IDictionary<string, string> parentState, ILogger logger)
        {
            if (logger == null)
                logger = _logger;
                                       
            var state = new Dictionary<string, string>();
            state.Merge(parentState);

            var operationTelemetry = CreateRequestOperation(operationName, state);

            try
            {
                state.SetParentRequestId(operationTelemetry.Telemetry.Id);

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
                operationTelemetry.Telemetry.Success = false;
                TelemetryClient.TrackException(e);
                throw;
            }
            finally
            {
                TelemetryClient.StopOperation(operationTelemetry);
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

            var operationTelemetry = CreateDependencyOperation(operationName, state);
            operationTelemetry.Telemetry.Type = type;

            try
            {
                state.SetParentRequestId(operationTelemetry.Telemetry.Id);

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
                operationTelemetry.Telemetry.Success = false;
                TelemetryClient.TrackException(e);
                throw;
            }
            finally
            {
                TelemetryClient.StopOperation(operationTelemetry);
            }

            bool LogError(Exception ex)
            {
                logger.LogError(ex, "Ein Fehler ist aufgetreten {Message}", ex.GetBaseException().Message);
                return true;
            }
        }
        
        public IOperationHolder<DependencyTelemetry> CreateDependencyOperation(
            string operationName,
            IDictionary<string, string> state)
        {
            IOperationHolder<DependencyTelemetry> operationTelemetry;

            var operationId = state.GetOperationId();
            var parentRequestId = state.GetParentRequestId();

            if (!string.IsNullOrEmpty(operationId) && !string.IsNullOrEmpty(parentRequestId))
            {
                operationTelemetry = TelemetryClient.StartOperation<DependencyTelemetry>(operationName
                    , operationId, parentRequestId);
            }
            else
            {
                operationTelemetry = TelemetryClient.StartOperation<DependencyTelemetry>(operationName);
            }

            operationTelemetry.Telemetry.Properties.Merge(state);
            return operationTelemetry;
        }

        public IOperationHolder<RequestTelemetry> CreateRequestOperation(
            string operationName,
            IDictionary<string, string> state)
        {
            IOperationHolder<RequestTelemetry> operationTelemetry;

            var operationId = state.GetOperationId();
            var parentRequestId = state.GetParentRequestId();

            if (!string.IsNullOrEmpty(operationId) && !string.IsNullOrEmpty(parentRequestId))
            {
                operationTelemetry =
                    TelemetryClient.StartOperation<RequestTelemetry>(operationName, operationId, parentRequestId);
            }
            else
            {
                operationTelemetry = TelemetryClient.StartOperation<RequestTelemetry>(operationName);
            }

            operationTelemetry.Telemetry.Properties.Merge(state);
            return operationTelemetry;
        }

      

       
    }
}