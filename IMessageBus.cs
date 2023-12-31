using System;
using System.Threading;
using System.Threading.Tasks;

namespace CMA.MessageBus
{
    public interface IMessageBus
    {
        Task ProducerAsync<T>(string topic, T message) where T : class;
        Task ConsumerAsync<T>(string topic, Func<T, Task> onMessage, CancellationToken cancellationToken) where T : class;

    }
}