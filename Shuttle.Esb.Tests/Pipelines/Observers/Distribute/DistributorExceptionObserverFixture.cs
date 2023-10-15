using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DistributorExceptionObserverFixture
{
    [Test]
    public void Should_be_able_to_handle_queue_distributor_exceptions_that_are_already_handled()
    {
        Should_be_able_to_handle_queue_distributor_exceptions_that_are_already_handled_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_handle_queue_distributor_exceptions_that_are_already_handled_async()
    {
        await Should_be_able_to_handle_queue_distributor_exceptions_that_are_already_handled_async(false);
    }

    private async Task Should_be_able_to_handle_queue_distributor_exceptions_that_are_already_handled_async(bool sync)
    {
        var errorQueue = new Mock<IQueue>();
        var workQueue = new Mock<IQueue>();

        var observer = new DistributorExceptionObserver(new Mock<IServiceBusPolicy>().Object, new Mock<ISerializer>().Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(new HandleExceptionObserver()) // marks exception as handled
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        pipeline.State.Add(StateKeys.WorkQueue, workQueue.Object);
        pipeline.State.Add(StateKeys.ErrorQueue, errorQueue.Object);

        if (sync)
        {
            pipeline.Execute(CancellationToken.None);
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }

        Assert.That(pipeline.Aborted, Is.True);

        errorQueue.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_handle_queue_distributor_exceptions_missing_message_state()
    {
        Should_be_able_to_handle_queue_distributor_exceptions_missing_message_state_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_handle_queue_distributor_exceptions_missing_message_state_async()
    {
        await Should_be_able_to_handle_queue_distributor_exceptions_missing_message_state_async(false);
    }

    private async Task Should_be_able_to_handle_queue_distributor_exceptions_missing_message_state_async(bool sync)
    {
        var errorQueue = new Mock<IQueue>();
        var workQueue = new Mock<IQueue>();

        var observer = new DistributorExceptionObserver(new Mock<IServiceBusPolicy>().Object, new Mock<ISerializer>().Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver()) // does not mark exception as handled
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        pipeline.State.Add(StateKeys.WorkQueue, workQueue.Object);
        pipeline.State.Add(StateKeys.ErrorQueue, errorQueue.Object);

        if (sync)
        {
            pipeline.Execute(CancellationToken.None);
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }

        Assert.That(pipeline.Aborted, Is.True);

        errorQueue.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_handle_queue_exception_retries_with_error_queue()
    {
        Should_be_able_to_handle_queue_exception_retries_with_error_queue_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_handle_queue_exception_retries_with_error_queue_async()
    {
        await Should_be_able_to_handle_queue_exception_retries_with_error_queue_async(false);
    }

    private async Task Should_be_able_to_handle_queue_exception_retries_with_error_queue_async(bool sync)
    {
        var policy = new Mock<IServiceBusPolicy>();

        policy.Setup(m => m.EvaluateMessageDistributionFailure(It.IsAny<OnPipelineException>()))
            .Returns(new MessageFailureAction(true, TimeSpan.Zero));

        var errorQueue = new Mock<IQueue>();
        var workQueue = new Mock<IQueue>();

        workQueue.Setup(m => m.IsStream).Returns(false);

        var observer = new DistributorExceptionObserver(policy.Object, new Mock<ISerializer>().Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        var transportMessage = new TransportMessage();

        pipeline.State.Add(StateKeys.ReceivedMessage, new ReceivedMessage(Stream.Null, Guid.NewGuid()));
        pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
        pipeline.State.Add(StateKeys.WorkQueue, workQueue.Object);
        pipeline.State.Add(StateKeys.ErrorQueue, errorQueue.Object);

        if (sync)
        {
            pipeline.Execute(CancellationToken.None);

            workQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(It.IsAny<object>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);

            workQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(It.IsAny<object>()), Times.Once);
        }

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_handle_queue_exception_failures_without_error_queue()
    {
        Should_be_able_to_handle_queue_exception_failures_without_error_queue_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_handle_queue_exception_failures_without_error_queue_async()
    {
        await Should_be_able_to_handle_queue_exception_failures_without_error_queue_async(false);
    }

    private async Task Should_be_able_to_handle_queue_exception_failures_without_error_queue_async(bool sync)
    {
        var policy = new Mock<IServiceBusPolicy>();

        policy.Setup(m => m.EvaluateMessageDistributionFailure(It.IsAny<OnPipelineException>()))
            .Returns(new MessageFailureAction(false, TimeSpan.Zero));

        var workQueue = new Mock<IQueue>();

        workQueue.Setup(m => m.IsStream).Returns(false);

        var observer = new DistributorExceptionObserver(policy.Object, new Mock<ISerializer>().Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        var transportMessage = new TransportMessage();

        pipeline.State.Add(StateKeys.ReceivedMessage, new ReceivedMessage(Stream.Null, Guid.NewGuid()));
        pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
        pipeline.State.Add(StateKeys.WorkQueue, workQueue.Object);

        if (sync)
        {
            pipeline.Execute(CancellationToken.None);

            workQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(It.IsAny<object>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);

            workQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(It.IsAny<object>()), Times.Once);
        }

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        workQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_handle_queue_exception_failures_with_error_queue()
    {
        Should_be_able_to_handle_queue_exception_failures_with_error_queue_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_handle_queue_exception_failures_with_error_queue_async()
    {
        await Should_be_able_to_handle_queue_exception_failures_with_error_queue_async(false);
    }

    private async Task Should_be_able_to_handle_queue_exception_failures_with_error_queue_async(bool sync)
    {
        var policy = new Mock<IServiceBusPolicy>();

        policy.Setup(m => m.EvaluateMessageDistributionFailure(It.IsAny<OnPipelineException>()))
            .Returns(new MessageFailureAction(false, TimeSpan.Zero));

        var errorQueue = new Mock<IQueue>();
        var workQueue = new Mock<IQueue>();

        workQueue.Setup(m => m.IsStream).Returns(false);

        var observer = new DistributorExceptionObserver(policy.Object, new Mock<ISerializer>().Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        var transportMessage = new TransportMessage();

        pipeline.State.Add(StateKeys.ReceivedMessage, new ReceivedMessage(Stream.Null, Guid.NewGuid()));
        pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
        pipeline.State.Add(StateKeys.WorkQueue, workQueue.Object);
        pipeline.State.Add(StateKeys.ErrorQueue, errorQueue.Object);

        if (sync)
        {
            pipeline.Execute(CancellationToken.None);

            errorQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(It.IsAny<object>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);

            errorQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(It.IsAny<object>()), Times.Once);
        }

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_handle_stream_exceptions()
    {
        Should_be_able_to_handle_stream_exceptions_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_handle_stream_exceptions_async()
    {
        await Should_be_able_to_handle_stream_exceptions_async(false);
    }

    private async Task Should_be_able_to_handle_stream_exceptions_async(bool sync)
    {
        var workQueue = new Mock<IQueue>();

        workQueue.Setup(m => m.IsStream).Returns(true);

        var observer = new DistributorExceptionObserver(new Mock<IServiceBusPolicy>().Object, new Mock<ISerializer>().Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        var transportMessage = new TransportMessage();

        pipeline.State.Add(StateKeys.ReceivedMessage, new ReceivedMessage(Stream.Null, Guid.NewGuid()));
        pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
        pipeline.State.Add(StateKeys.WorkQueue, workQueue.Object);

        if (sync)
        {
            pipeline.Execute(CancellationToken.None);

            workQueue.Verify(m => m.Release(It.IsAny<object>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);

            workQueue.Verify(m => m.ReleaseAsync(It.IsAny<object>()), Times.Once);
        }

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(pipeline.Aborted, Is.True);

        workQueue.VerifyNoOtherCalls();
    }
}