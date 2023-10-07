using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ReceiveExceptionObserverFixture : IPipelineObserver<OnException>
    {
        public void Execute(OnException pipelineEvent)
        {
            throw new Exception(string.Empty, new UnrecoverableHandlerException());
        }

        public Task ExecuteAsync(OnException pipelineEvent)
        {
            throw new Exception(string.Empty, new UnrecoverableHandlerException());
        }

        [Test]
        public void Should_be_able_to_move_message_to_error_queue_when_UnrecoverableHandlerException_is_thrown()
        {
            Should_be_able_to_move_message_to_error_queue_when_UnrecoverableHandlerException_is_thrown_async(true).GetAwaiter().GetResult();
        }

        [Test]
        public async Task Should_be_able_to_move_message_to_error_queue_when_UnrecoverableHandlerException_is_thrown_async()
        {
            await Should_be_able_to_move_message_to_error_queue_when_UnrecoverableHandlerException_is_thrown_async(false);
        }

        private async Task Should_be_able_to_move_message_to_error_queue_when_UnrecoverableHandlerException_is_thrown_async(bool sync)
        {
            var policy = new Mock<IServiceBusPolicy>();

            policy.Setup(m => m.EvaluateMessageHandlingFailure(It.IsAny<OnPipelineException>()))
                .Returns(new MessageFailureAction(true, TimeSpan.Zero));

            var errorQueue = new Mock<IQueue>();

            errorQueue.Setup(m => m.Uri).Returns(new QueueUri("queue://configuration/some-queue"));

            var observer = new ReceiveExceptionObserver(policy.Object,
                new Mock<ISerializer>().Object);

            var pipeline = new Pipeline()
                .RegisterObserver(this)
                .RegisterObserver(observer);

            pipeline
                .RegisterStage(".")
                .WithEvent<OnException>();

            var transportMessage = new TransportMessage();

            pipeline.State.Add(StateKeys.ReceivedMessage, new ReceivedMessage(Stream.Null, Guid.NewGuid()));
            pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
            pipeline.State.Add(StateKeys.WorkQueue, new Mock<IQueue>().Object);
            pipeline.State.Add(StateKeys.ErrorQueue, errorQueue.Object);

            if (sync)
            {
                pipeline.Execute(CancellationToken.None);

                errorQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            }
            else
            {
                await pipeline.ExecuteAsync(CancellationToken.None);

                errorQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            }
        }
    }
}