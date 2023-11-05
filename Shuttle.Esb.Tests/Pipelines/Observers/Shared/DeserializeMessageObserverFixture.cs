using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DeserializeMessageObserverFixture
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
        var serializer = new Mock<ISerializer>();

        var observer = new DeserializeMessageObserver(serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDeserializeMessage>();

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

        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_deserialize_message()
    {
        Should_be_able_to_deserialize_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_deserialize_message_async()
    {
        await Should_be_able_to_deserialize_message_async(false);
    }

    private async Task Should_be_able_to_deserialize_message_async(bool sync)
    {
        var serializer = new Mock<ISerializer>();

        var observer = new DeserializeMessageObserver(serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDeserializeMessage>();

        pipeline.State.SetTransportMessage(new()
        {
            AssemblyQualifiedName = typeof(TransportMessage).AssemblyQualifiedName,
            Message = new byte[] { }
        });

        if (sync)
        {
            pipeline.Execute();

            serializer.Verify(m => m.Deserialize(typeof(TransportMessage), It.IsAny<Stream>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            serializer.Verify(m => m.DeserializeAsync(typeof(TransportMessage), It.IsAny<Stream>()), Times.Once);
        }

        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_requeue_on_exception()
    {
        Should_be_able_to_requeue_on_exception_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_requeue_on_exception_async()
    {
        await Should_be_able_to_requeue_on_exception_async(false);
    }

    private async Task Should_be_able_to_requeue_on_exception_async(bool sync)
    {
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();
        var messageDeserializationExceptionCount = 0;

        var observer = new DeserializeMessageObserver(serializer.Object);

        observer.MessageDeserializationException += (sender, args) =>
        {
            messageDeserializationExceptionCount++;
        };

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDeserializeMessage>();

        var transportMessageType = typeof(TransportMessage);
        var receivedMessage = new ReceivedMessage(Stream.Null, Guid.NewGuid());
        var transportMessage = new TransportMessage
        {
            AssemblyQualifiedName = transportMessageType.AssemblyQualifiedName,
            Message = new byte[] { }
        };

        pipeline.State.SetTransportMessage(transportMessage);

        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);
        pipeline.State.SetReceivedMessage(receivedMessage);

        serializer.Setup(m => m.Deserialize(transportMessageType, It.IsAny<Stream>())).Throws<Exception>();
        serializer.Setup(m => m.DeserializeAsync(transportMessageType, It.IsAny<Stream>())).Throws<Exception>();

        workQueue.Setup(m => m.IsStream).Returns(false);

        if (sync)
        {
            pipeline.Execute();

            serializer.Verify(m => m.Deserialize(typeof(TransportMessage), It.IsAny<Stream>()), Times.Once);
            serializer.Verify(m => m.Serialize(It.IsAny<object>()), Times.Once);
            errorQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.Acknowledge(receivedMessage.AcknowledgementToken), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            serializer.Verify(m => m.DeserializeAsync(typeof(TransportMessage), It.IsAny<Stream>()), Times.Once);
            serializer.Verify(m => m.SerializeAsync(It.IsAny<object>()), Times.Once);
            errorQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);
        }

        workQueue.Verify(m => m.IsStream, Times.Once);

        Assert.That(messageDeserializationExceptionCount, Is.EqualTo(1));

        serializer.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
    }
}