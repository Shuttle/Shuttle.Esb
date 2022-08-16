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

            Assert.AreEqual(SubscribeType.Normal, options.Subscription.SubscribeType);
            Assert.AreEqual("connection-string", options.Subscription.ConnectionStringName);
            Assert.AreEqual("message-type-a", options.Subscription.MessageTypes[0]);
            Assert.AreEqual("message-type-b", options.Subscription.MessageTypes[1]);
        }
    }
}