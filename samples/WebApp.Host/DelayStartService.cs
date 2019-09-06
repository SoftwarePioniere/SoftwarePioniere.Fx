using System.Threading;
using System.Threading.Tasks;
using SoftwarePioniere.Hosting;

namespace WebApp.Host
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DelayStartService : ISopiService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken);
        }
    }
}
