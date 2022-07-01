using System;
using System.Linq;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class DefaultMessageRouteProviderFixture
    {
        [Test]
        public void Should_be_able_to_add_or_merge_message_routes()
        {
            var uri = new Uri("null://queue/");
            const string firstMessageType = "first-message-type";
            const string secondMessageType = "second-message-type";

            var provider = new DefaultMessageRouteProvider();

            Assert.IsFalse(provider.GetRouteUris(firstMessageType).Any());

            provider.Add(
                new MessageRoute(uri).AddSpecification(
                    new StartsWithMessageRouteSpecification("first")));

            Assert.IsTrue(provider.GetRouteUris(firstMessageType).Any());
            Assert.IsFalse(provider.GetRouteUris(secondMessageType).Any());
            Assert.AreEqual(uri, provider.GetRouteUris(firstMessageType).First());

            provider.Add(
                new MessageRoute(uri).AddSpecification(
                    new StartsWithMessageRouteSpecification("second")));

            Assert.IsTrue(provider.GetRouteUris(firstMessageType).Any());
            Assert.IsTrue(provider.GetRouteUris(secondMessageType).Any());
            Assert.AreEqual(uri, provider.GetRouteUris(firstMessageType).First());
            Assert.AreEqual(uri, provider.GetRouteUris(secondMessageType).First());
        }
    }
}