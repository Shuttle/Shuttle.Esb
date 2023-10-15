using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.System;

namespace Shuttle.Esb.Tests
{
    public class DeserializeTransportMessageObserverFixture
    {
        [Test]
        public void Should_be_able_to_kill_process_when_corrupt_message_is_received()
        {
            var serviceBusOptions = Microsoft.Extensions.Options.Options.Create(new ServiceBusOptions
            {
                RemoveCorruptMessages = false
            });
            var workQueue = new Mock<IQueue>();
            var errorQueue = new Mock<IQueue>();
            var serializer = new Mock<ISerializer>();
            var processService = new Mock<IProcessService>();
            var process = new Mock<IProcess>();

            workQueue.Setup(m => m.Uri).Returns(new QueueUri("queue://configuration/work-queue"));
            errorQueue.Setup(m => m.Uri).Returns(new QueueUri("queue://configuration/error-queue"));
            serializer.Setup(m => m.Deserialize(It.IsAny<Type>(), It.IsAny<Stream>())).Throws<Exception>();
            processService.Setup(m => m.GetCurrentProcess()).Returns(process.Object);

            var observer = new DeserializeTransportMessageObserver(
                serviceBusOptions,
                serializer.Object,
                new Mock<IEnvironmentService>().Object,
                processService.Object);

            var pipeline = new Pipeline()
                .RegisterObserver(observer);

            pipeline
                .RegisterStage(".")
                .WithEvent<OnDeserializeTransportMessage>();

            var transportMessage = new TransportMessage();

            pipeline.State.Add(StateKeys.ReceivedMessage, new ReceivedMessage(Stream.Null, Guid.NewGuid()));
            pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
            pipeline.State.Add(StateKeys.WorkQueue, workQueue.Object);
            pipeline.State.Add(StateKeys.ErrorQueue, errorQueue.Object);
            pipeline.Execute(CancellationToken.None);

            process.Verify(m => m.Kill(), Times.Once);
            workQueue.Verify(m => m.AcknowledgeAsync(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void Should_be_able_to_acknowledge_message_when_corrupt_message_is_received()
        {
            Should_be_able_to_acknowledge_message_when_corrupt_message_is_received_async(true).GetAwaiter().GetResult();
        }

        public async Task Should_be_able_to_acknowledge_message_when_corrupt_message_is_received_async()
        {
            await Should_be_able_to_acknowledge_message_when_corrupt_message_is_received_async(false);
        }

        private async Task Should_be_able_to_acknowledge_message_when_corrupt_message_is_received_async(bool sync)
        {
            var serviceBusOptions = Microsoft.Extensions.Options.Options.Create(new ServiceBusOptions
            {
                RemoveCorruptMessages = true
            });
            var workQueue = new Mock<IQueue>();
            var errorQueue = new Mock<IQueue>();
            var serializer = new Mock<ISerializer>();
            var processService = new Mock<IProcessService>();
            var process = new Mock<IProcess>();

            workQueue.Setup(m => m.Uri).Returns(new QueueUri("queue://configuration/work-queue"));
            errorQueue.Setup(m => m.Uri).Returns(new QueueUri("queue://configuration/error-queue"));
            serializer.Setup(m => m.Deserialize(It.IsAny<Type>(), It.IsAny<Stream>())).Throws<Exception>();
            processService.Setup(m => m.GetCurrentProcess()).Returns(process.Object);

            var observer = new DeserializeTransportMessageObserver(
                serviceBusOptions,
                serializer.Object,
                new Mock<IEnvironmentService>().Object,
                processService.Object);

            var pipeline = new Pipeline()
                .RegisterObserver(observer);

            pipeline
                .RegisterStage(".")
                .WithEvent<OnDeserializeTransportMessage>();

            var transportMessage = new TransportMessage();

            pipeline.State.Add(StateKeys.ReceivedMessage, new ReceivedMessage(Stream.Null, Guid.NewGuid()));
            pipeline.State.Add(StateKeys.TransportMessage, transportMessage);
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

            process.Verify(m => m.Kill(), Times.Never);

            if (sync)
            {
                workQueue.Verify(m => m.Acknowledge(It.IsAny<object>()), Times.Once);
            }
            else
            {
                workQueue.Verify(m => m.AcknowledgeAsync(It.IsAny<object>()), Times.Once);
            }
        }
    }
}