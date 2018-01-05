using System;
using Moq;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class MessageRoutesFixture
    {
        [Test]
        public void Should_be_able_to_create_new_routes()
        {
            var route = new MessageRoute(new Uri("route://"));
            var routes = new MessageRouteCollection();

            route.AddSpecification(new RegexMessageRouteSpecification("simple"));

            routes.Add(route);

            Assert.AreSame(route.Uri, routes.FindAll(new SimpleCommand().GetType().FullName)[0].Uri);
        }
    }
}