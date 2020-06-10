using System;
using System.ComponentModel;
using Xunit.Abstractions;

namespace KafkaMessaging.IntegrationTests.Infrastructure
{
    public class TestBase
    {
        public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(5);

        public ITestOutputHelper Logger { get; }

        public TestBase(ITestOutputHelper logger)
        {
            Logger = logger;
        }
    }
}
