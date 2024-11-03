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
    public async Task Should_be_able_to_return_when_message_does_not_need_to_be_deferred_async()
    {
        var deferredMessageProcessor = new Mock<IDeferredMessageProcessor>();
        var workQueue = new Mock<IQueue>();
        var deferredQueue = new Mock<IQueue>();

        var observer = new DeferTransportMessageObserver(deferredMessageProcessor.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAfterDeserializeTransportMessage>();

        pipeline.State.SetTransportMessage(new());
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetDeferredQueue(deferredQueue.Object);

        workQueue.SetupSequence(m => m.IsStream)
            .Returns(false)
            .Returns(true);

        await pipeline.ExecuteAsync();

        Assert.That(pipeline.Aborted, Is.False);

        workQueue.VerifyNoOtherCalls();
        deferredQueue.VerifyNoOtherCalls();
        deferredMessageProcessor.VerifyNoOtherCalls();

        await pipeline.ExecuteAsync();

        Assert.That(pipeline.Aborted, Is.False);

        workQueue.VerifyNoOtherCalls();
        deferredQueue.VerifyNoOtherCalls();
        deferredMessageProcessor.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_defer_message_to_work_queue_async()
    {
        var deferredMessageProcessor = new Mock<IDeferredMessageProcessor>();
        var workQueue = new Mock<IQueue>();

        var observer = new DeferTransportMessageObserver(deferredMessageProcessor.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
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

        await pipeline.ExecuteAsync();

        workQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
        workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        workQueue.VerifyNoOtherCalls();
        deferredMessageProcessor.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_defer_message_to_deferred_queue_async()
    {
        var deferredMessageProcessor = new Mock<IDeferredMessageProcessor>();
        var workQueue = new Mock<IQueue>();
        var deferredQueue = new Mock<IQueue>();

        var observer = new DeferTransportMessageObserver(deferredMessageProcessor.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
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

        await pipeline.ExecuteAsync();

        deferredQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
        workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);
        deferredMessageProcessor.Verify(m => m.MessageDeferredAsync(It.IsAny<DateTime>()), Times.Once);

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        workQueue.VerifyNoOtherCalls();
        deferredQueue.VerifyNoOtherCalls();
        deferredMessageProcessor.VerifyNoOtherCalls();
    }
}