using System;
using System.Threading;
using Castle.Windsor;
using NUnit.Framework;
using Shuttle.Core.Castle;
using Shuttle.Core.Container;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ServiceBusFixture
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
                }
            };

            var container = new WindsorComponentContainer(new WindsorContainer());

            container.Register<IMessageHandlerInvoker>(handlerInvoker);

            ServiceBus.Register(container, configuration);

            using (var bus = ServiceBus.Create(container))
            {
                bus.Start();

                var timeout = DateTime.Now.AddMilliseconds(1000);

                while (fakeQueue.MessageCount < 2 && DateTime.Now < timeout)
                {
                    Thread.Sleep(5);
                }
            }

            Assert.AreEqual(1, handlerInvoker.GetInvokeCount("SimpleCommand"),
                "FakeHandlerInvoker was not invoked exactly once.");
            Assert.AreEqual(2, fakeQueue.MessageCount, "FakeQueue was not invoked exactly twice.");
        }
    }
}