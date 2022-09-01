using System;
using System.Security.Principal;
using Microsoft.Extensions.Options;
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
            var transportMessage = new TransportMessage
            {
                SenderInboxWorkQueueUri = "null-queue://./work-queue"
            };
            var builder = new TransportMessageBuilder(transportMessage);

            var queueService = new Mock<IQueueService>();

            queueService.Setup(m => m.Get(It.IsAny<string>())).Returns((Uri uri) => new NullQueue(uri));

            identityProvider.Setup(m => m.Get()).Returns(new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous"));

            Assert.AreEqual("null-queue://./work-queue", transportMessage.SenderInboxWorkQueueUri);

            builder.WithSender("null-queue://./another-queue");

            Assert.AreEqual("null-queue://./another-queue", transportMessage.SenderInboxWorkQueueUri);
        }
    }
}