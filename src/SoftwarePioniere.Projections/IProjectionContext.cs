namespace SoftwarePioniere.Projections
{
    public interface IProjectionContext
    {
        bool IsLiveProcessing { get; }

        string StreamName { get; }
    }
}
