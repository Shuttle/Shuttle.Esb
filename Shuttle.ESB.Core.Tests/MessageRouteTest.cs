using Moq;
using NUnit.Framework;
using OtherNamespace;

namespace Shuttle.ESB.Core.Tests
{
	[TestFixture]
	public class MessageRouteTest
	{
		[Test]
		public void Should_be_able_to_create_a_new_route()
		{
			var map = new MessageRoute(new Mock<IQueue>().Object);

			map.AddSpecification(new RegexMessageRouteSpecification("simple"));

			Assert.IsFalse(map.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName));
			Assert.IsTrue(map.IsSatisfiedBy(new SimpleCommand().GetType().FullName));
		}
	}
}