using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
	[TestFixture]
	public class ServiceBusTest
	{
		[Test]
		public void Should_be_able_to_handle_expired_message()
		{
			var handlerInvoker = new FakeMessageHandlerInvoker();
			var fakeQueue = new FakeQueue(2);
			var configuration = new ServiceBusConfiguration
			{
				Inbox = new InboxQueueConfiguration
				{
					WorkQueue = fakeQueue,
					ErrorQueue = fakeQueue,
					ThreadCount = 1
				},
				MessageHandlerInvoker = handlerInvoker
			};

			using (var bus = new ServiceBus(configuration))
			{
				bus.Start();

				var timeout = DateTime.Now.AddMilliseconds(5000);

				while (fakeQueue.MessageCount < 2 && DateTime.Now < timeout)
				{
					Thread.Sleep(5);
				}
			}

			Assert.AreEqual(1, handlerInvoker.GetInvokeCount("SimpleCommand"), "FakeHandlerInvoker was not invoked exactly once.");
			Assert.AreEqual(2, fakeQueue.MessageCount, "FakeQueue was not invoked exactly twice.");
		}
	}
}