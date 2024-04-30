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
    public void Should_throw_exception_on_invariant_failure()
    {
        Should_throw_exception_on_invariant_failure_async(true);
    }

    [Test]
    public void Should_throw_exception_on_invariant_failure_async()
    {
        Should_throw_exception_on_invariant_failure_async(false);
    }

    private void Should_throw_exception_on_invariant_failure_async(bool sync)
    {
        var queueService = new Mock<IQueueService>();

        var observer = new SendOutboxMessageObserver(queueService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDispatchTransportMessage>();

        Core.Pipelines.PipelineException exception;

        if (sync)
        {
            exception = Assert.Throws<Core.Pipelines.PipelineException>(() => pipeline.Execute());
        }
        else
        {
            exception = Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync());
        }

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException?.Message, Contains.Substring(StateKeys.TransportMessage));

        pipeline.State.SetTransportMessage(new TransportMessage());

        if (sync)
        {
            exception = Assert.Throws<Core.Pipelines.PipelineException>(() => pipeline.Execute());
        }
        else
        {
            exception = Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync());
        }

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException?.Message, Contains.Substring(StateKeys.ReceivedMessage));

        pipeline.State.SetReceivedMessage(new ReceivedMessage(Stream.Null, Guid.NewGuid()));

        if (sync)
        {
            exception = Assert.Throws<Core.Pipelines.PipelineException>(() => pipeline.Execute());
        }
        else
        {
            exception = Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync());
        }

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException?.Message, Contains.Substring(nameof(TransportMessage.RecipientInboxWorkQueueUri)));

        queueService.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_enqueue_into_recipient_queue()
    {
        Should_be_able_to_enqueue_into_recipient_queue_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_enqueue_into_recipient_queue_async()
    {
        await Should_be_able_to_enqueue_into_recipient_queue_async(false);
    }

    private async Task Should_be_able_to_enqueue_into_recipient_queue_async(bool sync)
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
        pipeline.State.SetReceivedMessage(new ReceivedMessage(Stream.Null, Guid.NewGuid()));

        queueService.Setup(m => m.Get(It.IsAny<Uri>())).Returns(recipientQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            recipientQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()));
        }
        else
        {
            await pipeline.ExecuteAsync();

            recipientQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()));
        }

        queueService.Verify(m => m.Get(It.IsAny<Uri>()), Times.Once);

        queueService.VerifyNoOtherCalls();
        recipientQueue.VerifyNoOtherCalls();
    }
}