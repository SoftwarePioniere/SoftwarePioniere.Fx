using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.Domain
{
    public static class LoggerExtensions
    {
        public static void LogAggregate(this ILogger logger, AggregateRoot agg)
        {
            foreach (var change in agg.GetUncommittedChanges())
            {
                logger.LogDebug("{AggregateName} {AggregateId} {Event}", agg.GetType().GetAggregateName(), agg.AggregateId, change.ToString());
            }
        }
    }
}