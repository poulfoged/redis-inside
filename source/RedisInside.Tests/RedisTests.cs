using System.Diagnostics;
using NUnit.Framework;
using StackExchange.Redis;

namespace ElasticsearchInside.Tests
{
    [TestFixture]
    public class RedisTests
    {
        [Test]
        public void Can_configure()
        {
            using (var redis = new Redis(i => i.Port(1234).LogTo(message => Trace.WriteLine(message))))
            {
                Assert.That(redis.Node.EndsWith("1234"));
            }

        }

        [Test]
        public void Can_start()
        {
            using (var redis = new Redis())
            {
                var client = ConnectionMultiplexer.Connect(redis.Node);

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
            {
                var client = ConnectionMultiplexer.Connect(redis.Node + "," + redis2.Node);

                client.GetDatabase().StringSet("key", "value");

                var value = client.GetDatabase().StringGet("key");

                Assert.That(value.ToString(), Is.EqualTo("value"));
            }

        }
    }
}