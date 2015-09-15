using System.Linq;
using NUnit.Framework;

namespace Shuttle.ESB.Core.Tests
{
	[TestFixture]
	public class DefaultMessageRouteProviderTest
	{
		[Test]
		public void Should_be_able_to_add_or_merge_message_routes()
		{
			const string nullQueueUri = "null://queue/";
			const string firstMessageType = "first-message-type";
			const string secondMessageType = "second-message-type";

			var provider = new DefaultMessageRouteProvider();

			Assert.IsFalse(provider.GetRouteUris(firstMessageType).Any());

			provider.AddMessageRoute(new MessageRoute(new NullQueue(nullQueueUri)).AddSpecification(new StartsWithMessageRouteSpecification("first")));

			Assert.IsTrue(provider.GetRouteUris(firstMessageType).Any());
			Assert.IsFalse(provider.GetRouteUris(secondMessageType).Any());
			Assert.AreEqual(nullQueueUri, provider.GetRouteUris(firstMessageType).First());

			provider.AddMessageRoute(new MessageRoute(new NullQueue(nullQueueUri)).AddSpecification(new StartsWithMessageRouteSpecification("second")));

			Assert.IsTrue(provider.GetRouteUris(firstMessageType).Any());
			Assert.IsTrue(provider.GetRouteUris(secondMessageType).Any());
			Assert.AreEqual(nullQueueUri, provider.GetRouteUris(firstMessageType).First());
			Assert.AreEqual(nullQueueUri, provider.GetRouteUris(secondMessageType).First());
		}
	}
}