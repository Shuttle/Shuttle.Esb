using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.System;

namespace Shuttle.Esb.Tests.Shared
{
    public class DeserializeTransportMessageObserverFixture
    {
        [Test]
        public void Should_be_able_to_kill_process_when_corrupt_message_is_received()
        {
            var configuration = new Mock<IServiceBusConfiguration>();
            var brokerEndpoint = new Mock<IBrokerEndpoint>();
            var errorBrokerEndpoint = new Mock<IBrokerEndpoint>();
            var serializer = new Mock<ISerializer>();
            var processService = new Mock<IProcessService>();
            var process = new Mock<IProcess>();

            configuration.Setup(m => m.RemoveCorruptMessages).Returns(false);
            brokerEndpoint.Setup(m => m.Uri).Returns(new Uri("queue://work-queue"));
            errorBrokerEndpoint.Setup(m => m.Uri).Returns(new Uri("queue://error-queue"));
            serializer.Setup(m => m.Deserialize(It.IsAny<Type>(), It.IsAny<Stream>())).Throws<Exception>();
            processService.Setup(m => m.GetCurrentProcess()).Returns(process.Object);

            var observer = new DeserializeTransportMessageObserver(
                configuration.Object,
                new Mock<IServiceBusEvents>().Object,
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
            pipeline.State.Add(StateKeys.BrokerEndpoint, brokerEndpoint.Object);
            pipeline.State.Add(StateKeys.ErrorBrokerEndpoint, errorBrokerEndpoint.Object);
            pipeline.Execute();

            process.Verify(m => m.Kill(), Times.Once);
            brokerEndpoint.Verify(m => m.Acknowledge(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void Should_be_able_to_acknowledge_message_when_corrupt_message_is_received()
        {
            var configuration = new Mock<IServiceBusConfiguration>();
            var brokerEndpoint = new Mock<IBrokerEndpoint>();
            var errorBrokerEndpoint = new Mock<IBrokerEndpoint>();
            var serializer = new Mock<ISerializer>();
            var processService = new Mock<IProcessService>();
            var process = new Mock<IProcess>();

            configuration.Setup(m => m.RemoveCorruptMessages).Returns(true);
            brokerEndpoint.Setup(m => m.Uri).Returns(new Uri("queue://work-queue"));
            errorBrokerEndpoint.Setup(m => m.Uri).Returns(new Uri("queue://error-queue"));
            serializer.Setup(m => m.Deserialize(It.IsAny<Type>(), It.IsAny<Stream>())).Throws<Exception>();
            processService.Setup(m => m.GetCurrentProcess()).Returns(process.Object);

            var observer = new DeserializeTransportMessageObserver(
                configuration.Object,
                new Mock<IServiceBusEvents>().Object,
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
            pipeline.State.Add(StateKeys.BrokerEndpoint, brokerEndpoint.Object);
            pipeline.State.Add(StateKeys.ErrorBrokerEndpoint, errorBrokerEndpoint.Object);
            pipeline.Execute();

            process.Verify(m => m.Kill(), Times.Never);
            brokerEndpoint.Verify(m => m.Acknowledge(It.IsAny<object>()), Times.Once);
        }
    }
}