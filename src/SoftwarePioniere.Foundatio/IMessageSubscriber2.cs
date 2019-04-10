using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Foundatio.Messaging
{
    public interface IMessageSubscriber2 : IMessageSubscriber
    {
        Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, Func<T, bool> messagefilter, CancellationToken cancellationToken = default) where T : class;
    }

    public static class MessageBusExtensions
    {
        public static Task SubscribeAsync<T>(this IMessageSubscriber2 subscriber, Func<T, Task> handler, CancellationToken cancellationToken = default, Func<T, bool> messagefilter = null) where T : class
        {
            return subscriber.SubscribeAsync<T>((msg, token) => handler(msg), cancellationToken);
        }

        public static Task SubscribeAsync<T>(this IMessageSubscriber2 subscriber, Action<T> handler, CancellationToken cancellationToken = default, Func<T, bool> messagefilter = null) where T : class
        {
            return subscriber.SubscribeAsync<T>((msg, token) =>
            {
                handler(msg);
                return Task.CompletedTask;
            }, cancellationToken);
        }
    }
}
