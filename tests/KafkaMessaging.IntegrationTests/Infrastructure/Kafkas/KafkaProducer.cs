using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Xunit.Abstractions;

namespace KafkaMessaging.IntegrationTests.Infrastructure.Kafkas
{
    public class KafkaProducer : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ITestOutputHelper _logger;

        public KafkaProducer(ITestOutputHelper logger, string topic)
        {
            _topic = topic ?? throw new ArgumentException($"{nameof(topic)} is null.");
            _logger = logger ?? throw new ArgumentException($"{nameof(logger)} is null.");

            var producerConfig = new ProducerConfig 
            { 
                BootstrapServers = ConfigurationManager.Configuration["KAFKA_LOCAL_HOST"] 
            };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public void SendMany(IEnumerable<object> objectList)
        {
            foreach (var o in objectList)
            {
                var message = KafkaMessage.Create(o);
                this.Send(message, message.Key);
            }
            
            _producer.Flush(TimeSpan.FromSeconds(3));
        }

        private void Send(
            Message<string, string> message, 
            string messageId)
        {
            _producer.Produce(
                _topic, 
                message, 
                (deliveryReport) =>
                {
                    _logger.WriteLine(deliveryReport.Error.Code == ErrorCode.NoError
                        ? $"{messageId} sent to {deliveryReport.TopicPartitionOffset}"
                        : $"Failed to deliver {messageId}: {deliveryReport.Error.Reason}");
                });
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
