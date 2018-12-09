using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ReceiveExceptionObserverFixture : IPipelineObserver<OnTest>
    {
        public void Execute(OnTest pipelineEvent)
        {
            throw new Exception(string.Empty, new UnrecoverableHandlerException());
        }

        [Test]
        public void Should_be_able_to_move_message_to_error_queue_when_UnrecoverableHandlerException_is_thrown()
        {
            var policy = new Mock<IServiceBusPolicy>();

            policy.Setup(m => m.EvaluateMessageHandlingFailure(It.IsAny<OnPipelineException>()))
                .Returns(new MessageFailureAction(true, TimeSpan.Zero));

            var errorQueue = new Mock<IQueue>();

            errorQueue.Setup(m => m.Uri).Returns(new Uri("queue://some-queue"));

            var observer = new ReceiveExceptionObserver(
                new Mock<IServiceBusEvents>().Object,
                policy.Object,
                new Mock<ISerializer>().Object);

            var pipeline = new Pipeline()
                .RegisterObserver(this)
                .RegisterObserver(observer);

            pipeline
                .RegisterStage(".")
                .WithEvent<OnTest>();

            var transportMessage = new TransportMessage();

            pipeline.State.Add(StateKeys.ReceivedMessage, new ReceivedMessage(Stream.Null, Guid.NewGuid()));
            pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
            pipeline.State.Add(StateKeys.WorkQueue, new Mock<IQueue>().Object);
            pipeline.State.Add(StateKeys.ErrorQueue, errorQueue.Object);
            pipeline.Execute();

            errorQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
        }
    }
}