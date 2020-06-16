using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Contracts.KafkaMessages;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace KafkaMessaging.IntegrationTests.Infrastructure.Kafkas
{
    public class KafkaConsumer : IDisposable
    {
        private const string CONTRACTS_ASSEMBLY_NAME = "Contracts.KafkaMessages";

        private readonly ITestOutputHelper _logger;
        private readonly string _topic;
        private readonly IConsumer<string, string> _consumer;

        private readonly Lazy<Type[]> _assemblyTypes = new Lazy<Type[]>(()=> 
            AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(x=> x.FullName.Contains(CONTRACTS_ASSEMBLY_NAME))
                .SelectMany(x => x.GetTypes())
                .ToArray());


        public KafkaConsumer(ITestOutputHelper logger, string topic)
        {
            _logger = logger ?? throw new ApplicationException($"{nameof(logger)} is null.");
            _topic = topic ?? throw new ApplicationException($"{nameof(topic)} is null.");

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = ConfigurationManager.Configuration["KAFKA_LOCAL_HOST"],
                GroupId = $"IntegrationTests.Consumer.{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                    .SetErrorHandler((_, error) => _logger.WriteLine(error.Reason))
                    .SetLogHandler((_, message) => _logger.WriteLine(message.Message))
                    .Build();

            _consumer.Subscribe(_topic);
        }

        public async IAsyncEnumerable<object> StartEnumerableConsumingAsync(
            Guid customerId, 
            CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null) throw new ApplicationException($"{nameof(cancellationTokenSource)} is null.");

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var consumeResult = await Task.Run(() =>
                {
                    try
                    {
                        return _consumer.Consume(cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Correct consumer shutdown.
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLine(e.Message);
                    }

                    return null;
                });

                if (consumeResult?.Message?.Key != null 
                    && consumeResult.Message.Value != null
                    && consumeResult.Message.Headers != null)
                {
                    var result = this.DeserealizeConsumeResult(customerId, consumeResult);
                    if (result != null)
                    {
                        yield return result;
                    }
                }
            }
        }

        private object DeserealizeConsumeResult(
            Guid customerId, 
            [NotNull] ConsumeResult<string, string> consumeResult)
        {
            object response = null;
            
            var msg = consumeResult.Message;
            if (string.Equals(msg.Key, customerId.ToString(), StringComparison.OrdinalIgnoreCase))
            { 
                var headers = msg.Headers.ToDictionary(kv => kv.Key, kv => Encoding.UTF8.GetString(kv.GetValueBytes()));
                string typeName = headers[KafkaHeaders.Type];
                response = this.DeserealizeByTypeName(msg.Value, typeName);
            }

            return response;
        }

        private object DeserealizeByTypeName(string jsonMessage, string typeName)
        {
            Type type = _assemblyTypes.Value.FirstOrDefault(t => t.Name == typeName) 
                        ?? throw new NotSupportedException($"Type {typeName} is not supported.");
            
            return JsonConvert.DeserializeObject(jsonMessage, type);
        }

        public void Dispose()
        {
            _consumer.Close();
            _consumer?.Dispose();
        }
    }
}
