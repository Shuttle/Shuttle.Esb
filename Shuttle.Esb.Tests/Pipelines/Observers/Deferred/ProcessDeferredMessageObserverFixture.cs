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
    public void Should_be_able_to_process_a_deferred_message_when_ready()
    {
        Should_be_able_to_process_a_deferred_message_when_ready_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_process_a_deferred_message_when_ready_async()
    {
        await Should_be_able_to_process_a_deferred_message_when_ready_async(false);
    }

    private async Task Should_be_able_to_process_a_deferred_message_when_ready_async(bool sync)
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

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.State.GetDeferredMessageReturned, Is.False);

        if (sync)
        {
            deferredQueue.Verify(m => m.Release(It.IsAny<object>()), Times.Once);
        }
        else
        {
            deferredQueue.Verify(m => m.ReleaseAsync(It.IsAny<object>()), Times.Once);
        }

        deferredQueue.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();

        await Task.Delay(TimeSpan.FromMilliseconds(200));

        workQueue = new Mock<IQueue>();
        deferredQueue = new Mock<IQueue>();
        
        pipeline.State.Clear();
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetDeferredQueue(deferredQueue.Object);
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetTransportMessage(transportMessage);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.State.GetDeferredMessageReturned, Is.True);

        if (sync)
        {
            deferredQueue.Verify(m => m.Acknowledge(It.IsAny<object>()), Times.Once);
            workQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
        }
        else
        {
            deferredQueue.Verify(m => m.AcknowledgeAsync(It.IsAny<object>()), Times.Once);
            workQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
        }

        deferredQueue.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
    }
}