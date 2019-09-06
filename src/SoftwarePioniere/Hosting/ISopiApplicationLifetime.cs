using System.Threading;

namespace SoftwarePioniere.Hosting
{
    public interface ISopiApplicationLifetime
    {
        CancellationToken Stopped { get; }

        void Stop();

        bool IsStarted { get; }

        bool IsStarting { get; }

    }
}
