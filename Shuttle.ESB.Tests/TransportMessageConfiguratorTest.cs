using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
	[TestFixture]
	public class TransportMessageConfiguratorTest
	{
		[Test]
		public void Should_be_able_to_set_sender()
		{
			var configurator = new TransportMessageConfigurator(this);
			var configuration = new ServiceBusConfiguration
			{
				Inbox = new InboxQueueConfiguration
				{
					WorkQueue = new NullQueue("null-queue://./work-queue")
				}
			};

			Assert.AreEqual("null-queue://./work-queue", configurator.TransportMessage(configuration).SenderInboxWorkQueueUri);

			configurator.WithSender("null-queue://./another-queue");

			Assert.AreEqual("null-queue://./another-queue", configurator.TransportMessage(configuration).SenderInboxWorkQueueUri);
		}
	}
}