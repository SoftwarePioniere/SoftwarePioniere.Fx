using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public interface IProjectionContext
    {
        IEntityStore EntityStore { get; }

        ProjectionStatus Status { get; }

        string ProjectorId { get; }

        long CurrentCheckPoint { get; }

        bool IsLiveProcessing { get; }

        string StreamName { get; }

        bool IsReady { get; }

        bool IsInitializing { get; }
    }
}
