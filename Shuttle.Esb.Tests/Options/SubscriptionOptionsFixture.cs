using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class SubscriptionOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);

            Assert.That(options.Subscription.SubscribeType, Is.EqualTo(SubscribeType.Normal));
            Assert.That(options.Subscription.ConnectionStringName, Is.EqualTo("connection-string"));
            Assert.That(options.Subscription.CacheTimeout, Is.EqualTo(new TimeSpan(0, 0, 7, 15)));
            Assert.That(options.Subscription.MessageTypes[0], Is.EqualTo("message-type-a"));
            Assert.That(options.Subscription.MessageTypes[1], Is.EqualTo("message-type-b"));
        }
    }
}