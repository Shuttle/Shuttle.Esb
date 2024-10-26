using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class SendDeferredObserverFixture
{
    [Test]
    public async Task Should_be_able_to_ignore_sending_deferred_messages_async()
    {
        var pipelineFactory = new Mock<IPipelineFactory>();
        var serializer = new Mock<ISerializer>();
        var idempotenceService = new Mock<IIdempotenceService>();

        var observer = new SendDeferredObserver(pipelineFactory.Object, serializer.Object, idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnSendDeferred>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        await pipeline.ExecuteAsync();

        pipelineFactory.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Ignore));
    }

    [Test]
    public async Task Should_be_able_to_ignore_completing_processing_async()
    {
        var pipelineFactory = new Mock<IPipelineFactory>();
        var serializer = new Mock<ISerializer>();
        var idempotenceService = new Mock<IIdempotenceService>();

        var observer = new SendDeferredObserver(pipelineFactory.Object, serializer.Object, idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAfterSendDeferred>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        await pipeline.ExecuteAsync();

        pipelineFactory.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Ignore));
    }

    [Test]
    public async Task Should_be_able_to_complete_processing_async()
    {
        var pipelineFactory = new Mock<IPipelineFactory>();
        var serializer = new Mock<ISerializer>();
        var idempotenceService = new Mock<IIdempotenceService>();
        var transportMessage = new TransportMessage();

        var observer = new SendDeferredObserver(pipelineFactory.Object, serializer.Object, idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAfterSendDeferred>();

        pipeline.State.SetTransportMessage(transportMessage);

        await pipeline.ExecuteAsync();

        idempotenceService.Verify(m => m.ProcessingCompletedAsync(transportMessage), Times.Once);

        pipelineFactory.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Active));
    }

    [Test]
    public async Task Should_be_able_to_send_deferred_messages_async()
    {
        var pipelineFactory = new Mock<IPipelineFactory>();
        var serializer = new Mock<ISerializer>();
        var idempotenceService = new Mock<IIdempotenceService>();
        var messagePipeline = new DispatchTransportMessagePipeline(new Mock<IFindMessageRouteObserver>().Object, new Mock<ISerializeTransportMessageObserver>().Object, new Mock<IDispatchTransportMessageObserver>().Object);
        var transportMessage = new TransportMessage();
        var deferredTransportMessage = new TransportMessage();

        idempotenceService.Setup(m => m.GetDeferredMessagesAsync(transportMessage)).Returns(Task.FromResult(new List<Stream> { new MemoryStream() }.AsEnumerable()));

        serializer.Setup(m => m.DeserializeAsync(It.IsAny<Type>(), It.IsAny<Stream>())).Returns(Task.FromResult((object)deferredTransportMessage));

        pipelineFactory.Setup(m => m.GetPipeline<DispatchTransportMessagePipeline>()).Returns(messagePipeline);

        var observer = new SendDeferredObserver(pipelineFactory.Object, serializer.Object, idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnSendDeferred>();

        pipeline.State.SetTransportMessage(transportMessage);

        await pipeline.ExecuteAsync();

        idempotenceService.Verify(m => m.GetDeferredMessagesAsync(transportMessage), Times.Once);
        serializer.Verify(m => m.DeserializeAsync(typeof(TransportMessage), It.IsAny<Stream>()), Times.Once);
        idempotenceService.Verify(m => m.DeferredMessageSentAsync(transportMessage, deferredTransportMessage));

        pipelineFactory.Verify(m => m.GetPipeline<DispatchTransportMessagePipeline>(), Times.Once);
        pipelineFactory.Verify(m => m.ReleasePipeline(messagePipeline), Times.Once);

        pipelineFactory.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Active));
    }
}