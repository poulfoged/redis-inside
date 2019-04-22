using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;

namespace RedisInside.Tests
{
    [TestFixture]
    public class RedisTests
    {
        [Test]
        public void Can_configure()
        {
            using (var redis = new Redis(new TestConfig(1234)))
            {
                Assert.That(redis.Endpoint.ToString().EndsWith("1234"));
            }

        }

        [Test]
        public void Can_start()
        {
            using (var redis = new Redis())
            using (var client = ConnectionMultiplexer.Connect(redis.Endpoint.ToString()))
            {
                client.GetDatabase().StringSet("key", "value");

                var value = client.GetDatabase().StringGet("key");

                Assert.That(value.ToString(), Is.EqualTo("value"));
            }

        }

        [Test]
        public void Can_start_multiple()
        {
            using (var redis = new Redis())
            using (var redis2 = new Redis())
            using (var client = ConnectionMultiplexer.Connect(redis.Endpoint + "," + redis2.Endpoint))
            {
                client.GetDatabase().StringSet("key", "value");
                var value = client.GetDatabase().StringGet("key");

                Assert.That(value.ToString(), Is.EqualTo("value"));
            }
        }

        [Test]
        public async Task Can_start_slave()
        {

            using (var redis = new Redis())
            using (var redis2 = new Redis())
            {
                ////Arrange
                // configure slave
                var config = new ConfigurationOptions { AllowAdmin = true };
                config.EndPoints.Add(redis.Endpoint);
                config.EndPoints.Add(redis2.Endpoint);
                using (var client = ConnectionMultiplexer.Connect(config))
                    await client.GetServer(redis.Endpoint).SlaveOfAsync(redis2.Endpoint);

                // new single-node client
                string actualValue;
                using (var client = ConnectionMultiplexer.Connect(redis2.Endpoint.ToString()))
                {

                    await client.GetDatabase().StringSetAsync("key", "value");

                    ////Act
                    actualValue = await client.GetDatabase().StringGetAsync("key");
                }

                ////Assert
                Assert.That(actualValue, Is.EqualTo("value"));
            }
        }

        private class TestConfig : IConfig
        {
            public int Port { get; }

            public string LinuxTemporaryPath => null;

            public string WindowsTemporaryPath => null;

            public string LinuxExecutablePath => null;

            public string WindowsExecutablePath => null;

            public TestConfig(int port)
            {
                this.Port = port;
            }

            public void Log(string message)
            {
                Trace.WriteLine(message);
            }
        }
    }
}