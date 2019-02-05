using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftwarePioniere.Messaging
{
    public interface IHandleWithState<in T> where T : IMessage
    {
        Task HandleAsync(T message, IDictionary<string, string> state);
    }
}