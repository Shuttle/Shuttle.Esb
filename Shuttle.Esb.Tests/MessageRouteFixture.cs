using System;
using Moq;
using NUnit.Framework;
using Shuttle.Esb.Tests.Messages;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class MessageRouteFixture
    {
        [Test]
        public void Should_be_able_to_create_a_new_route()
        {
            var map = new MessageRoute(new Uri("route://"));

            map.AddSpecification(new RegexMessageRouteSpecification("simple"));

            Assert.IsFalse(map.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName));
            Assert.IsTrue(map.IsSatisfiedBy(new SimpleCommand().GetType().FullName));
        }
    }
}