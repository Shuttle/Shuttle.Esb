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
public class ReceiveExceptionObserverFixture : IPipelineObserver<OnException>
{
    public async Task ExecuteAsync(IPipelineContext<OnException> pipelineContext)
    {
        await Task.CompletedTask;

        throw new(string.Empty, new UnrecoverableHandlerException());
    }

    [Test]
    public async Task Should_be_able_to_move_message_to_error_queue_when_UnrecoverableHandlerException_is_thrown_async()
    {
        var policy = new Mock<IServiceBusPolicy>();

        policy.Setup(m => m.EvaluateMessageHandlingFailure(It.IsAny<IPipelineContext<OnPipelineException>>()))
            .Returns(new MessageFailureAction(true, TimeSpan.Zero));

        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();

        errorQueue.Setup(m => m.Uri).Returns(new QueueUri("queue://configuration/some-queue"));

        var observer = new ReceiveExceptionObserver(policy.Object,
            new Mock<ISerializer>().Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(this)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnException>();

        var transportMessage = new TransportMessage();
        var receivedMessage = new ReceivedMessage(Stream.Null, Guid.NewGuid());

        pipeline.State.Add(StateKeys.ReceivedMessage, receivedMessage);
        pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
        pipeline.State.Add(StateKeys.WorkQueue, workQueue.Object);
        pipeline.State.Add(StateKeys.ErrorQueue, errorQueue.Object);

        await pipeline.ExecuteAsync(CancellationToken.None);

        workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken), Times.Once);
        errorQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
    }
}