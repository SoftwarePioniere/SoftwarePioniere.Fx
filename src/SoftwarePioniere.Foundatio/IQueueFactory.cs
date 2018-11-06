using Foundatio.Queues;

namespace SoftwarePioniere.Foundatio
{
    public interface IQueueFactory
    {
        IQueue<T> CreateQueue<T>(string name) where T : class;
    }
}