using System.Threading;

namespace SoftwarePioniere.Hosting
{
    public interface ISopiApplicationLifetime
    {
        CancellationToken Stopped { get; }

        void Stop();


        bool IsStopped { get; }

        bool IsStarted { get; }

        bool IsStarting { get; }

    }
}
