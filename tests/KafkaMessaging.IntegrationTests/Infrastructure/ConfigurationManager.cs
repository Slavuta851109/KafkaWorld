using Microsoft.Extensions.Configuration;

namespace KafkaMessaging.IntegrationTests.Infrastructure
{
    public static class ConfigurationManager
    {
        public static readonly IConfiguration Configuration;

        static ConfigurationManager()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }
    }
}
