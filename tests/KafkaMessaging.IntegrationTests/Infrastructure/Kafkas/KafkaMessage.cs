using System;
using System.Text;
using Confluent.Kafka;
using Contracts.KafkaMessages;
using Contracts.KafkaMessages.WalletTransactions;
using Newtonsoft.Json;

namespace KafkaMessaging.IntegrationTests.Infrastructure.Kafkas
{
    public static class KafkaMessage
    {
        private const string SOURCE = "IntegrationTests";

        public static Message<string, string> Create(object o)
        {
            switch (o)
            {
                case WalletTransaction t:
                    return Create_WalletTransaction(t);
                default:
                    throw new NotImplementedException($"Type {o.GetType()} is not supported for message mapping.");
            }
        }

        public static Message<string, string> Create_WalletTransaction(WalletTransaction objectToSend)
        {
            if (objectToSend.BrandName == null) throw new ArgumentException($"{nameof(objectToSend.BrandName)} cannot be null.");

            Message<string, string> message = new Message<string, string>
            {
                Key = objectToSend.CustomerId.ToString(),
                Value = JsonConvert.SerializeObject(objectToSend, Formatting.None)
            };

            string MESSAGE_TYPE = objectToSend.GetType().Name;
            string BRAND_NAME = objectToSend.BrandName.ToLower();
            message.Headers = new Headers
            {
                { KafkaHeaders.Source, Encoding.UTF8.GetBytes(SOURCE) },
                { KafkaHeaders.Type, Encoding.UTF8.GetBytes(MESSAGE_TYPE) },
                { KafkaHeaders.BrandName, Encoding.UTF8.GetBytes(BRAND_NAME) },
            };

            return message;
        }
    }
}
