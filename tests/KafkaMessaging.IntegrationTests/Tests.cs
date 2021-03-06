using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Contracts.KafkaMessages.WalletTransactions;
using KafkaMessaging.IntegrationTests.Infrastructure;
using KafkaMessaging.IntegrationTests.Infrastructure.Kafkas;
using Xunit;
using Xunit.Abstractions;

namespace KafkaMessaging.IntegrationTests
{
    public class Tests : TestBase
    {
        public Tests(ITestOutputHelper logger) 
            : base(logger)
        {
        }

        [Fact]
        public async void Test1()
        {
            var walletTransaction = new Fixture().Create<WalletTransaction>();
            walletTransaction.BrandName = Brands.Brand1;

            Guid customerId = walletTransaction.CustomerId;

            using (var producer = new KafkaProducer(this.Logger, KafkaTopics.TopicToProduce))
            {
                producer.SendMany(new[] { walletTransaction });
            }

            int expectedMessageCount = 1;
            int counter = 0;
            using (var consumer = new KafkaConsumer(this.Logger, KafkaTopics.TopicToProduce))
            {
                var cts = new CancellationTokenSource(this.DefaultTimeout);
                await foreach (var messageObject in consumer.StartEnumerableConsumingAsync(customerId, cts).ConfigureAwait(false))
                {
                    if (messageObject is WalletTransaction message)
                    {
                        Assert.Equal(message.BrandName, walletTransaction.BrandName);
                        Assert.Equal(message.CustomerId, walletTransaction.CustomerId);
                        Assert.Equal(message.TransactionId, walletTransaction.TransactionId);
                        Assert.Equal(message.Amount, walletTransaction.Amount);
                        Assert.Equal(message.Balance, walletTransaction.Balance);
                        Assert.Equal(message.Timestamp, walletTransaction.Timestamp);

                        if (++counter == expectedMessageCount)
                        {
                            cts.Cancel();
                        }
                    }
                }
            }

            Assert.Equal(expectedMessageCount, counter);
        }

    }
}
