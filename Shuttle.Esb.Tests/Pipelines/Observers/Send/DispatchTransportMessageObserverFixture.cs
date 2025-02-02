using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DispatchTransportMessageObserverFixture
{
    [Test]
    public async Task Should_be_able_to_dispatch_message_to_outbox_async()
    {
        var serviceBusConfiguration = new Mock<IServiceBusConfiguration>();
        var queueService = new Mock<IQueueService>();
        var outboxConfiguration = new Mock<IOutboxConfiguration>();
        var recipientQueue = new Mock<IQueue>();
        var outboxQueue = new Mock<IQueue>();
        var transportMessage = new TransportMessage
        {
            RecipientInboxWorkQueueUri = "queue://recipient/work-queue"
        };
        var transportMessageStream = Stream.Null;

        outboxConfiguration.Setup(m => m.WorkQueue).Returns(outboxQueue.Object);
        serviceBusConfiguration.Setup(m => m.Outbox).Returns(outboxConfiguration.Object);
        queueService.Setup(m => m.Get(new Uri(transportMessage.RecipientInboxWorkQueueUri))).Returns(recipientQueue.Object);

        var observer = new DispatchTransportMessageObserver(serviceBusConfiguration.Object, queueService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnDispatchTransportMessage>();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetTransportMessageStream(transportMessageStream);

        await pipeline.ExecuteAsync();

        outboxQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);

        serviceBusConfiguration.VerifyNoOtherCalls();
        queueService.VerifyNoOtherCalls();
        outboxConfiguration.VerifyNoOtherCalls();
        recipientQueue.VerifyNoOtherCalls();
        outboxQueue.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_dispatch_message_to_recipient_async()
    {
        var serviceBusConfiguration = new Mock<IServiceBusConfiguration>();
        var queueService = new Mock<IQueueService>();
        var recipientQueue = new Mock<IQueue>();
        var outboxQueue = new Mock<IQueue>();
        var transportMessage = new TransportMessage
        {
            RecipientInboxWorkQueueUri = "queue://recipient/work-queue"
        };
        var transportMessageStream = Stream.Null;

        queueService.Setup(m => m.Get(transportMessage.RecipientInboxWorkQueueUri)).Returns(recipientQueue.Object);

        var observer = new DispatchTransportMessageObserver(serviceBusConfiguration.Object, queueService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnDispatchTransportMessage>();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetTransportMessageStream(transportMessageStream);

        await pipeline.ExecuteAsync();

        recipientQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);

        queueService.VerifyNoOtherCalls();
        recipientQueue.VerifyNoOtherCalls();
        outboxQueue.VerifyNoOtherCalls();
    }
}