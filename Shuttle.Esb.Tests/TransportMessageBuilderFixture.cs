using System;
using System.Security.Principal;
using Moq;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class TransportMessageBuilderFixture
    {
        [Test]
        public void Should_be_able_to_set_sender()
        {
            var serviceBusOptions = new ServiceBusOptions();
            var identityProvider = new Mock<IIdentityProvider>();
            var builder = new TransportMessageBuilder(this);
            var serviceBusConfiguration = new ServiceBusConfiguration
            {
                Inbox = new InboxConfiguration
                {
                    WorkQueue = new NullQueue("null-queue://./work-queue")
                }
            };

            identityProvider.Setup(m => m.Get()).Returns(new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous"));


            Assert.AreEqual("null-queue://./work-queue",
                builder.TransportMessage(serviceBusOptions, serviceBusConfiguration, identityProvider.Object).SenderInboxWorkQueueUri);

            builder.WithSender("null-queue://./another-queue");

            Assert.AreEqual("null-queue://./another-queue",
                builder.TransportMessage(serviceBusOptions, serviceBusConfiguration, identityProvider.Object).SenderInboxWorkQueueUri);
        }
    }
}