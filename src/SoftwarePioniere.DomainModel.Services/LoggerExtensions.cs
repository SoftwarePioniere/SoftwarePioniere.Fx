using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.DomainModel
{
    public static class LoggerExtensions
    {
        public static void LogAggregate(this ILogger logger, AggregateRoot agg)
        {
            foreach (var change in agg.GetUncommittedChanges())
            {
                logger.LogInformation("{AggregateName} {AggregateId} {Event}", agg.GetType().GetAggregateName(), agg.Id, change.ToString());
            }
        }
    }
}