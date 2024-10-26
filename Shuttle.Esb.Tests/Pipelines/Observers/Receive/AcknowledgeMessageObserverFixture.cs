using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class AcknowledgeMessageObserverFixture
{
    [Test]
    public async Task Should_be_able_to_ignore_acknowledgement_on_failures_async()
    {
        var observer = new AcknowledgeMessageObserver();

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(new HandleExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>()
            .WithEvent<OnAcknowledgeMessage>();

        var workQueue = new Mock<IQueue>();

        pipeline.State.SetWorkQueue(workQueue.Object);

        await pipeline.ExecuteAsync();

        workQueue.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_acknowledge_message_async()
    {
        var observer = new AcknowledgeMessageObserver();

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAcknowledgeMessage>();

        var workQueue = new Mock<IQueue>();
        var receivedMessage = new ReceivedMessage(new Mock<Stream>().Object, Guid.NewGuid());

        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetReceivedMessage(receivedMessage);

        await pipeline.ExecuteAsync();

        workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);

        workQueue.VerifyNoOtherCalls();
    }
}