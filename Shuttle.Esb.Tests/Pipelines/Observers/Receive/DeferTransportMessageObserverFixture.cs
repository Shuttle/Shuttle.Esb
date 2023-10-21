using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DeferTransportMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_return_when_message_does_not_need_to_be_deferred()
    {
        Should_be_able_to_return_when_message_does_not_need_to_be_deferred_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_return_when_message_does_not_need_to_be_deferred_async()
    {
        await Should_be_able_to_return_when_message_does_not_need_to_be_deferred_async(false);
    }

    private async Task Should_be_able_to_return_when_message_does_not_need_to_be_deferred_async(bool sync)
    {
        var deferredMessageProcessor = new Mock<IDeferredMessageProcessor>();
        var workQueue = new Mock<IQueue>();
        var deferredQueue = new Mock<IQueue>();

        var observer = new DeferTransportMessageObserver(deferredMessageProcessor.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAfterDeserializeTransportMessage>();

        pipeline.State.SetTransportMessage(new TransportMessage());
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetDeferredQueue(deferredQueue.Object);

        workQueue.SetupSequence(m => m.IsStream)
            .Returns(false)
            .Returns(true);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.Aborted, Is.False);

        workQueue.VerifyNoOtherCalls();
        deferredQueue.VerifyNoOtherCalls();
        deferredMessageProcessor.VerifyNoOtherCalls();

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.Aborted, Is.False);

        workQueue.VerifyNoOtherCalls();
        deferredQueue.VerifyNoOtherCalls();
        deferredMessageProcessor.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_defer_message_to_work_queue()
    {
        Should_be_able_to_defer_message_to_work_queue_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_defer_message_to_work_queue_async()
    {
        await Should_be_able_to_defer_message_to_work_queue_async(false);
    }

    private async Task Should_be_able_to_defer_message_to_work_queue_async(bool sync)
    {
        var deferredMessageProcessor = new Mock<IDeferredMessageProcessor>();
        var workQueue = new Mock<IQueue>();

        var observer = new DeferTransportMessageObserver(deferredMessageProcessor.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAfterDeserializeTransportMessage>();

        var transportMessage = new TransportMessage { IgnoreTillDate = DateTime.Now.AddDays(1) };
        var receivedMessage = new ReceivedMessage(new MemoryStream(), Guid.NewGuid());

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);

        workQueue.Setup(m => m.IsStream).Returns(false);

        if (sync)
        {
            pipeline.Execute();

            workQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(receivedMessage.AcknowledgementToken), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            workQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);
        }

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        workQueue.VerifyNoOtherCalls();
        deferredMessageProcessor.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_defer_message_to_deferred_queue()
    {
        Should_be_able_to_defer_message_to_deferred_queue_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_defer_message_to_deferred_queue_async()
    {
        await Should_be_able_to_defer_message_to_deferred_queue_async(false);
    }

    private async Task Should_be_able_to_defer_message_to_deferred_queue_async(bool sync)
    {
        var deferredMessageProcessor = new Mock<IDeferredMessageProcessor>();
        var workQueue = new Mock<IQueue>();
        var deferredQueue = new Mock<IQueue>();

        var observer = new DeferTransportMessageObserver(deferredMessageProcessor.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAfterDeserializeTransportMessage>();

        var transportMessage = new TransportMessage { IgnoreTillDate = DateTime.Now.AddDays(1) };
        var receivedMessage = new ReceivedMessage(new MemoryStream(), Guid.NewGuid());

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetDeferredQueue(deferredQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            deferredQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(receivedMessage.AcknowledgementToken), Times.Once);
            deferredMessageProcessor.Verify(m => m.MessageDeferred(It.IsAny<DateTime>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            deferredQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);
            deferredMessageProcessor.Verify(m => m.MessageDeferredAsync(It.IsAny<DateTime>()), Times.Once);
        }

        workQueue.Verify(m => m.IsStream, Times.Once);
        
        Assert.That(pipeline.Aborted, Is.True);

        workQueue.VerifyNoOtherCalls();
        deferredQueue.VerifyNoOtherCalls();
        deferredMessageProcessor.VerifyNoOtherCalls();
    }
}