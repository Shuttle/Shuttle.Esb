using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.System;

namespace Shuttle.Esb.Tests;

public class DeserializeTransportMessageObserverFixture
{
    [Test]
    public async Task Should_be_able_to_acknowledge_message_when_corrupt_message_is_received_async()
    {
        var serviceBusOptions = Options.Create(new ServiceBusOptions
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
        serializer.Setup(m => m.DeserializeAsync(It.IsAny<Type>(), It.IsAny<Stream>())).Throws<Exception>();
        processService.Setup(m => m.GetCurrentProcess()).Returns(process.Object);

        var observer = new DeserializeTransportMessageObserver(
            serviceBusOptions,
            serializer.Object,
            new Mock<IEnvironmentService>().Object,
            processService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnDeserializeTransportMessage>();

        var transportMessage = new TransportMessage();

        pipeline.State.SetReceivedMessage(new(Stream.Null, Guid.NewGuid()));
        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        await pipeline.ExecuteAsync();

        process.Verify(m => m.Kill(), Times.Never);

        workQueue.Verify(m => m.AcknowledgeAsync(It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_kill_process_when_corrupt_message_is_received_async()
    {
        var serviceBusOptions = Options.Create(new ServiceBusOptions
        {
            RemoveCorruptMessages = false
        });
        var workQueue = new Mock<IQueue>();
        var serializer = new Mock<ISerializer>();
        var processService = new Mock<IProcessService>();
        var process = new Mock<IProcess>();

        workQueue.Setup(m => m.Uri).Returns(new QueueUri("queue://configuration/work-queue"));
        serializer.Setup(m => m.DeserializeAsync(It.IsAny<Type>(), It.IsAny<Stream>())).Throws<Exception>();
        processService.Setup(m => m.GetCurrentProcess()).Returns(process.Object);

        var observer = new DeserializeTransportMessageObserver(
            serviceBusOptions,
            serializer.Object,
            new Mock<IEnvironmentService>().Object,
            processService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnDeserializeTransportMessage>();

        var transportMessage = new TransportMessage();

        pipeline.State.SetReceivedMessage(new(Stream.Null, Guid.NewGuid()));
        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetWorkQueue(workQueue.Object);

        await pipeline.ExecuteAsync(CancellationToken.None);

        serializer.Verify(m => m.DeserializeAsync(typeof(TransportMessage), It.IsAny<Stream>()), Times.Once);

        process.Verify(m => m.Kill(), Times.Once);

        workQueue.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }
}