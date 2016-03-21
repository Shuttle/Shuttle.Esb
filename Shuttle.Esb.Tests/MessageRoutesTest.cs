using System;
using Moq;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
	[TestFixture]
	public class MessageRoutesTest
	{
		[Test]
		public void Should_be_able_to_create_new_routes()
		{
			var queue = new Mock<IQueue>();
			var route = new MessageRoute(queue.Object);
			var routes = new MessageRouteCollection();

			queue.Setup(m => m.Uri).Returns(new Uri("uri://somewhere"));

			route.AddSpecification(new RegexMessageRouteSpecification("simple"));

			routes.Add(route);

			Assert.AreSame(queue.Object, routes.FindAll(new SimpleCommand().GetType().FullName)[0].Queue);
		}
	}
}