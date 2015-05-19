using NUnit.Framework;

namespace Shuttle.ESB.Core.Tests
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
					WorkQueue = new NullQueue("nq://./work-queue")
				}
			};

			Assert.AreEqual("nq://./work-queue", configurator.TransportMessage(configuration).SenderInboxWorkQueueUri);

			configurator.WithSender("nq://./another-queue");

			Assert.AreEqual("nq://./another-queue", configurator.TransportMessage(configuration).SenderInboxWorkQueueUri);
		}
	}
}