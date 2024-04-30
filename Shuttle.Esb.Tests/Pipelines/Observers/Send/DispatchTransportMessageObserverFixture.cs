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
    public void Should_be_able_to_defer_message()
    {
        Should_be_able_to_defer_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_defer_message_async()
    {
        await Should_be_able_to_defer_message_async(false);
    }

    private async Task Should_be_able_to_defer_message_async(bool sync)
    {
        var serviceBusConfiguration = new Mock<IServiceBusConfiguration>();
        var queueService = new Mock<IQueueService>();
        var idempotenceService = new Mock<IIdempotenceService>();
        var outboxConfiguration = new Mock<IOutboxConfiguration>();
        var recipientQueue = new Mock<IQueue>();
        var outboxQueue = new Mock<IQueue>();
        var transportMessage = new TransportMessage
        {
            RecipientInboxWorkQueueUri = "queue://recipient/work-queue"
        };
        var transportMessageStream = Stream.Null;
        var transportMessageReceived = new TransportMessage();

        outboxConfiguration.Setup(m => m.WorkQueue).Returns(outboxQueue.Object);
        serviceBusConfiguration.Setup(m => m.Outbox).Returns(outboxConfiguration.Object);
        queueService.Setup(m => m.Get(new Uri(transportMessage.RecipientInboxWorkQueueUri))).Returns(recipientQueue.Object);
        idempotenceService.Setup(m => m.AddDeferredMessage(transportMessageReceived, transportMessage, transportMessageStream)).Returns(true);
        idempotenceService.Setup(m => m.AddDeferredMessageAsync(transportMessageReceived, transportMessage, transportMessageStream)).Returns(ValueTask.FromResult(true));

        var observer = new DispatchTransportMessageObserver(serviceBusConfiguration.Object, queueService.Object, idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDispatchTransportMessage>();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetTransportMessageStream(transportMessageStream);
        pipeline.State.SetTransportMessageReceived(transportMessageReceived);

        if (sync)
        {
            pipeline.Execute();

            idempotenceService.Verify(m => m.AddDeferredMessage(transportMessageReceived, transportMessage, transportMessageStream), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            idempotenceService.Verify(m => m.AddDeferredMessageAsync(transportMessageReceived, transportMessage, transportMessageStream), Times.Once);
        }

        serviceBusConfiguration.VerifyNoOtherCalls();
        queueService.VerifyNoOtherCalls();
        idempotenceService.VerifyNoOtherCalls();
        outboxConfiguration.VerifyNoOtherCalls();
        recipientQueue.VerifyNoOtherCalls();
        outboxQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_dispatch_message_to_outbox()
    {
        Should_be_able_to_dispatch_message_to_outbox_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_dispatch_message_to_outbox_async()
    {
        await Should_be_able_to_dispatch_message_to_outbox_async(false);
    }

    private async Task Should_be_able_to_dispatch_message_to_outbox_async(bool sync)
    {
        var serviceBusConfiguration = new Mock<IServiceBusConfiguration>();
        var queueService = new Mock<IQueueService>();
        var idempotenceService = new Mock<IIdempotenceService>();
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

        var observer = new DispatchTransportMessageObserver(serviceBusConfiguration.Object, queueService.Object, idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDispatchTransportMessage>();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetTransportMessageStream(transportMessageStream);

        if (sync)
        {
            pipeline.Execute();

            outboxQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            outboxQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
        }

        serviceBusConfiguration.VerifyNoOtherCalls();
        queueService.VerifyNoOtherCalls();
        idempotenceService.VerifyNoOtherCalls();
        outboxConfiguration.VerifyNoOtherCalls();
        recipientQueue.VerifyNoOtherCalls();
        outboxQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_dispatch_message_to_recipient()
    {
        Should_be_able_to_dispatch_message_to_recipient_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_dispatch_message_to_recipient_async()
    {
        await Should_be_able_to_dispatch_message_to_recipient_async(false);
    }

    private async Task Should_be_able_to_dispatch_message_to_recipient_async(bool sync)
    {
        var serviceBusConfiguration = new Mock<IServiceBusConfiguration>();
        var queueService = new Mock<IQueueService>();
        var idempotenceService = new Mock<IIdempotenceService>();
        var recipientQueue = new Mock<IQueue>();
        var outboxQueue = new Mock<IQueue>();
        var transportMessage = new TransportMessage
        {
            RecipientInboxWorkQueueUri = "queue://recipient/work-queue"
        };
        var transportMessageStream = Stream.Null;

        queueService.Setup(m => m.Get(new Uri(transportMessage.RecipientInboxWorkQueueUri))).Returns(recipientQueue.Object);

        var observer = new DispatchTransportMessageObserver(serviceBusConfiguration.Object, queueService.Object, idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDispatchTransportMessage>();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetTransportMessageStream(transportMessageStream);

        if (sync)
        {
            pipeline.Execute();

            recipientQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            recipientQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
        }

        queueService.VerifyNoOtherCalls();
        idempotenceService.VerifyNoOtherCalls();
        recipientQueue.VerifyNoOtherCalls();
        outboxQueue.VerifyNoOtherCalls();
    }
}