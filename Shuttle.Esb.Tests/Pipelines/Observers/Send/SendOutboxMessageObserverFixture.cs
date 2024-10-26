using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class SendOutboxMessageObserverFixture
{
    [Test]
    public void Should_throw_exception_on_invariant_failure_async()
    {
        var queueService = new Mock<IQueueService>();

        var observer = new SendOutboxMessageObserver(queueService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDispatchTransportMessage>();

        var exception = Assert.ThrowsAsync<Core.Pipelines.PipelineException>(async () => await pipeline.ExecuteAsync())!;

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException?.Message, Contains.Substring(StateKeys.TransportMessage));

        pipeline.State.SetTransportMessage(new());

        exception = Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync())!;

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException?.Message, Contains.Substring(StateKeys.ReceivedMessage));

        pipeline.State.SetReceivedMessage(new(Stream.Null, Guid.NewGuid()));

        exception = Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync())!;

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException?.Message, Contains.Substring(nameof(TransportMessage.RecipientInboxWorkQueueUri)));

        queueService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_enqueue_into_recipient_queue_async()
    {
        var queueService = new Mock<IQueueService>();
        var recipientQueue = new Mock<IQueue>();

        var observer = new SendOutboxMessageObserver(queueService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDispatchTransportMessage>();

        var transportMessage = new TransportMessage { RecipientInboxWorkQueueUri = "queue://host/somewhere" };

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetReceivedMessage(new(Stream.Null, Guid.NewGuid()));

        queueService.Setup(m => m.Get(It.IsAny<string>())).Returns(recipientQueue.Object);

        await pipeline.ExecuteAsync();

        recipientQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()));

        queueService.Verify(m => m.Get(It.IsAny<string>()), Times.Once);

        queueService.VerifyNoOtherCalls();
        recipientQueue.VerifyNoOtherCalls();
    }
}