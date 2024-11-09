using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class IdempotenceObserverFixture
{
    [Test]
    public async Task Should_be_able_to_process_idempotence_message_async()
    {
        var idempotenceService = new Mock<IIdempotenceService>();
        var transportMessage = new TransportMessage();

        idempotenceService.Setup(m => m.ProcessingStatusAsync(transportMessage)).Returns(ValueTask.FromResult(ProcessingStatus.Assigned));

        var observer = new IdempotenceObserver(idempotenceService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnProcessIdempotenceMessage>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        await pipeline.ExecuteAsync();

        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Ignore));

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);

        await pipeline.ExecuteAsync();

        idempotenceService.Verify(m => m.ProcessingStatusAsync(transportMessage));

        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Assigned));
    }

    [Test]
    public async Task Should_be_able_to_handle_idempotence_message_async()
    {
        var idempotenceService = new Mock<IIdempotenceService>();
        var transportMessage = new TransportMessage();

        var observer = new IdempotenceObserver(idempotenceService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnIdempotenceMessageHandled>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        await pipeline.ExecuteAsync();

        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Ignore));

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);

        await pipeline.ExecuteAsync();

        idempotenceService.Verify(m => m.MessageHandledAsync(transportMessage), Times.Once);

        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Active));
    }
}