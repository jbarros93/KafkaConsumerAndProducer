using Confluent.Kafka;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CMA.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private readonly string _bootstrapServer;

        public MessageBus(string bootstrapServer)
        {
            _bootstrapServer = bootstrapServer;
        }

        public async Task ConsumerAsync<T>(
            string topic, 
            Func<T, Task> onMessage, 
            CancellationToken cancellationToken) 
            where T : class
        {
            _ = Task.Factory.StartNew(async () =>
            {
            var config = new ConsumerConfig
            {
                GroupId = "carlos-group",
                BootstrapServers = _bootstrapServer,
                EnableAutoCommit = false,
                EnablePartitionEof = true
            };

                using var consumer = new ConsumerBuilder<string, string>(config).Build();

                consumer.Subscribe(topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = consumer.Consume();

                    if (result.IsPartitionEOF)
                        continue;

                    var message = JsonSerializer.Deserialize<T>(result.Message.Value);

                    await onMessage(message);

                    consumer.Commit();
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            await Task.CompletedTask;
        }


        public async Task ProducerAsync<T>(string topic, T message) where T : class
        {
            var config = new ProducerConfig()
            {
                BootstrapServers = _bootstrapServer
            };

            var payload = JsonSerializer.Serialize(message);

            var producer = new ProducerBuilder<string, string>(config).Build();

            await producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = payload
            });
            await Task.CompletedTask;
        }
    }
}