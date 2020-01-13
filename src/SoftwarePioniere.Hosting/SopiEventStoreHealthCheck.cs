//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using EventStore.ClientAPI;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using SoftwarePioniere.EventStore;

//namespace SoftwarePioniere.Hosting
//{
//    public class SopiEventStoreHealthCheck : IHealthCheck
//    {
//        private readonly EventStoreConnectionProvider _provider;

//        public SopiEventStoreHealthCheck(EventStoreConnectionProvider provider)
//        {
//            _provider = provider;
//        }

//        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                //var settings = _provider.Connection.Value.Settings;
//                //var con = EventStoreConnection.Create(settings, );

//                var con = _provider.CreateNewConnection(builder =>
//                    builder.FailOnNoServerResponse()
//                        .LimitReconnectionsTo(2)
//                        .LimitAttemptsForOperationTo(2)
//                        .WithConnectionTimeoutOf(TimeSpan.FromSeconds(0.5))
//                );
//                await con.ConnectAsync().ConfigureAwait(false);

//                await con.ReadAllEventsForwardAsync(Position.Start, 1, false, _provider.AdminCredentials).ConfigureAwait(false);

//                con.Close();


//                return HealthCheckResult.Healthy();
//            }
//            catch (Exception ex)
//            {
//                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
//            }
//        }
//    }
//}