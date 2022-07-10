using System;
using System.Security.Principal;
using Moq;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class TransportMessageConfiguratorFixture
    {
        [Test]
        public void Should_be_able_to_set_sender()
        {
            var identityProvider = new Mock<IIdentityProvider>();
            var configurator = new TransportMessageConfigurator(this);
            var configuration = new ServiceBusConfiguration
            {
                Inbox = new InboxConfiguration
                {
                    WorkQueue = new NullQueue("null-queue://./work-queue")
                }
            };

            identityProvider.Setup(m => m.Get()).Returns(new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous"));


            Assert.AreEqual("null-queue://./work-queue",
                configurator.TransportMessage(configuration, identityProvider.Object).SenderInboxWorkQueueUri);

            configurator.WithSender("null-queue://./another-queue");

            Assert.AreEqual("null-queue://./another-queue",
                configurator.TransportMessage(configuration, identityProvider.Object).SenderInboxWorkQueueUri);
        }
    }
}