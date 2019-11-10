using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Polly;

namespace SoftwarePioniere.AzureCosmosDb
{
    class ThrottlingHandler : RequestHandler
    {
        private readonly ILogger _logger;

        public ThrottlingHandler(ILogger logger)
        {
            _logger = logger;
        }
        public override Task<ResponseMessage> SendAsync(
            RequestMessage request,
            CancellationToken cancellationToken)
        {
            return Policy
                .HandleResult<ResponseMessage>(r => (int)r.StatusCode == 429)
                //  .RetryForeverAsync((result, i) => _logger.LogWarning("Retry on Requeset too large {Retry}", i))

                //   .RetryForeverAsync((result, context) => {} )

                //.RetryForeverAsync((exception, i) => _logger.LogError(exception, "Retry {Retry}", i))
                .RetryAsync(3, (result, i) => _logger.LogWarning("Retry {Retry}", i))
                .ExecuteAsync(() => base.SendAsync(request, cancellationToken));
        }
    }
}