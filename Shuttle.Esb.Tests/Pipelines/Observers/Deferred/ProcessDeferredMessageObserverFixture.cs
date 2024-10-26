using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class ProcessDeferredMessageObserverFixture
{
    [Test]
    public async Task Should_be_able_to_process_a_deferred_message_when_ready_async()
    {
        var observer = new ProcessDeferredMessageObserver();

        var pipeline = new Pipeline().RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnProcessDeferredMessage>();

        var workQueue = new Mock<IQueue>();
        var deferredQueue = new Mock<IQueue>();

        var transportMessage = new TransportMessage
        {
            IgnoreTillDate = DateTime.Now.AddMilliseconds(200)
        };

        var receivedMessage = new ReceivedMessage(new MemoryStream(), Guid.NewGuid());

        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetDeferredQueue(deferredQueue.Object);
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetTransportMessage(transportMessage);

        await pipeline.ExecuteAsync();

        Assert.That(pipeline.State.GetDeferredMessageReturned, Is.False);

        deferredQueue.Verify(m => m.ReleaseAsync(It.IsAny<object>()), Times.Once);

        deferredQueue.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();

        await Task.Delay(TimeSpan.FromMilliseconds(200));

        workQueue = new();
        deferredQueue = new();

        pipeline.State.Clear();
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetDeferredQueue(deferredQueue.Object);
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetTransportMessage(transportMessage);

        await pipeline.ExecuteAsync();

        Assert.That(pipeline.State.GetDeferredMessageReturned, Is.True);

        deferredQueue.Verify(m => m.AcknowledgeAsync(It.IsAny<object>()), Times.Once);
        workQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);

        deferredQueue.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
    }
}