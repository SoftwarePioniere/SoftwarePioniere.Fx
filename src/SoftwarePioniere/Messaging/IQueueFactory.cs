using Foundatio.Queues;

namespace SoftwarePioniere.Messaging
{
    public interface IQueueFactory
    {
        IQueue<T> CreateQueue<T>(string name) where T : class;
    }
}