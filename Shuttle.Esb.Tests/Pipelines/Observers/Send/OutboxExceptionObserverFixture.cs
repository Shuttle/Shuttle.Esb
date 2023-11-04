using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class OutboxExceptionObserverFixture
{
    [Test]
    public void Should_be_able_to_skip_when_exception_has_been_handled()
    {
        Should_be_able_to_skip_when_exception_has_been_handled_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_skip_when_exception_has_been_handled_async()
    {
        await Should_be_able_to_skip_when_exception_has_been_handled_async(false);
    }

    private async Task Should_be_able_to_skip_when_exception_has_been_handled_async(bool sync)
    {
        var serviceBusPolicy = new Mock<IServiceBusPolicy>();
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();

        var observer = new OutboxExceptionObserver(serviceBusPolicy.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(new HandleExceptionObserver()) // marks exception as handled
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.Aborted, Is.True);

        serviceBusPolicy.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_skip_when_there_is_no_message_available()
    {
        Should_be_able_to_skip_when_there_is_no_message_available_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_skip_when_there_is_no_message_available_async()
    {
        await Should_be_able_to_skip_when_there_is_no_message_available_async(false);
    }

    private async Task Should_be_able_to_skip_when_there_is_no_message_available_async(bool sync)
    {
        var serviceBusPolicy = new Mock<IServiceBusPolicy>();
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();

        var observer = new OutboxExceptionObserver(serviceBusPolicy.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.Aborted, Is.True);

        serviceBusPolicy.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_release_when_there_is_no_transport_message_available()
    {
        Should_be_able_to_release_when_there_is_no_transport_message_available_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_release_when_there_is_no_transport_message_available_async()
    {
        await Should_be_able_to_release_when_there_is_no_transport_message_available_async(false);
    }

    private async Task Should_be_able_to_release_when_there_is_no_transport_message_available_async(bool sync)
    {
        var serviceBusPolicy = new Mock<IServiceBusPolicy>();
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();

        var observer = new OutboxExceptionObserver(serviceBusPolicy.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        var receivedMessage = new ReceivedMessage(Stream.Null, Guid.NewGuid());

        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            workQueue.Verify(m => m.Release(receivedMessage.AcknowledgementToken), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            workQueue.Verify(m => m.ReleaseAsync(receivedMessage.AcknowledgementToken), Times.Once);
        }

        Assert.That(pipeline.Aborted, Is.True);

        serviceBusPolicy.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_release_when_using_a_stream()
    {
        Should_be_able_to_release_when_using_a_stream_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_release_when_using_a_stream_async()
    {
        await Should_be_able_to_release_when_using_a_stream_async(false);
    }

    private async Task Should_be_able_to_release_when_using_a_stream_async(bool sync)
    {
        var serviceBusPolicy = new Mock<IServiceBusPolicy>();
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();

        var observer = new OutboxExceptionObserver(serviceBusPolicy.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        workQueue.Setup(m => m.IsStream).Returns(true);

        var receivedMessage = new ReceivedMessage(Stream.Null, Guid.NewGuid());

        pipeline.State.SetTransportMessage(new TransportMessage());
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            workQueue.Verify(m => m.Release(receivedMessage.AcknowledgementToken), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            workQueue.Verify(m => m.ReleaseAsync(receivedMessage.AcknowledgementToken), Times.Once);
        }

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        serviceBusPolicy.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_retry()
    {
        Should_be_able_to_retry_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_retry_async()
    {
        await Should_be_able_to_retry_async(false);
    }

    private async Task Should_be_able_to_retry_async(bool sync)
    {
        var serviceBusPolicy = new Mock<IServiceBusPolicy>();
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();

        var observer = new OutboxExceptionObserver(serviceBusPolicy.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        workQueue.Setup(m => m.IsStream).Returns(false);
        serviceBusPolicy.Setup(m => m.EvaluateOutboxFailure(It.IsAny<OnPipelineException>())).Returns(new MessageFailureAction(true, TimeSpan.Zero));

        var transportMessage = new TransportMessage();
        var receivedMessage = new ReceivedMessage(Stream.Null, Guid.NewGuid());

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            serializer.Verify(m => m.Serialize(transportMessage));
            workQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(receivedMessage.AcknowledgementToken), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            serializer.Verify(m => m.SerializeAsync(transportMessage));
            workQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);
        }

        serviceBusPolicy.Verify(m => m.EvaluateOutboxFailure(It.IsAny<OnPipelineException>()), Times.Once);
        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        serviceBusPolicy.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_not_retry()
    {
        Should_be_able_to_not_retry_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_not_retry_async()
    {
        await Should_be_able_to_not_retry_async(false);
    }

    private async Task Should_be_able_to_not_retry_async(bool sync)
    {
        var serviceBusPolicy = new Mock<IServiceBusPolicy>();
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();

        var observer = new OutboxExceptionObserver(serviceBusPolicy.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        workQueue.Setup(m => m.IsStream).Returns(false);
        serviceBusPolicy.Setup(m => m.EvaluateOutboxFailure(It.IsAny<OnPipelineException>())).Returns(new MessageFailureAction(false, TimeSpan.Zero));

        var transportMessage = new TransportMessage();
        var receivedMessage = new ReceivedMessage(Stream.Null, Guid.NewGuid());

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            serializer.Verify(m => m.Serialize(transportMessage));
            errorQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(receivedMessage.AcknowledgementToken), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            serializer.Verify(m => m.SerializeAsync(transportMessage));
            errorQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);
        }

        serviceBusPolicy.Verify(m => m.EvaluateOutboxFailure(It.IsAny<OnPipelineException>()), Times.Once);
        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        serviceBusPolicy.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_not_retry_with_no_error_queue()
    {
        Should_be_able_to_not_retry_with_no_error_queue_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_not_retry_with_no_error_queue_async()
    {
        await Should_be_able_to_not_retry_with_no_error_queue_async(false);
    }

    private async Task Should_be_able_to_not_retry_with_no_error_queue_async(bool sync)
    {
        var serviceBusPolicy = new Mock<IServiceBusPolicy>();
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();

        var observer = new OutboxExceptionObserver(serviceBusPolicy.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        workQueue.Setup(m => m.IsStream).Returns(false);
        serviceBusPolicy.Setup(m => m.EvaluateOutboxFailure(It.IsAny<OnPipelineException>())).Returns(new MessageFailureAction(false, TimeSpan.Zero));

        var transportMessage = new TransportMessage();
        var receivedMessage = new ReceivedMessage(Stream.Null, Guid.NewGuid());

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetReceivedMessage(receivedMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            serializer.Verify(m => m.Serialize(transportMessage));
            workQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(receivedMessage.AcknowledgementToken), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            serializer.Verify(m => m.SerializeAsync(transportMessage));
            workQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);
        }

        serviceBusPolicy.Verify(m => m.EvaluateOutboxFailure(It.IsAny<OnPipelineException>()), Times.Once);
        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        serviceBusPolicy.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
    }
}