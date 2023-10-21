using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class IdempotenceObserverFixture
{
    [Test]
    public void Should_be_able_to_process_idempotence_message()
    {
        Should_be_able_to_process_idempotence_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_process_idempotence_message_async()
    {
        await Should_be_able_to_process_idempotence_message_async(false);
    }

    private async Task Should_be_able_to_process_idempotence_message_async(bool sync)
    {
        var idempotenceService = new Mock<IIdempotenceService>();
        var transportMessage = new TransportMessage();

        idempotenceService.Setup(m => m.ProcessingStatus(transportMessage)).Returns(ProcessingStatus.Assigned);
        idempotenceService.Setup(m => m.ProcessingStatusAsync(transportMessage)).Returns(ValueTask.FromResult(ProcessingStatus.Assigned));

        var observer = new IdempotenceObserver(idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnProcessIdempotenceMessage>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Ignore));

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);

        if (sync)
        {
            pipeline.Execute();

            idempotenceService.Verify(m => m.ProcessingStatus(transportMessage));
        }
        else
        {
            await pipeline.ExecuteAsync();

            idempotenceService.Verify(m => m.ProcessingStatusAsync(transportMessage));
        }

        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Assigned));
    }

    [Test]
    public void Should_be_able_to_handle_idempotence_message()
    {
        Should_be_able_to_handle_idempotence_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_handle_idempotence_message_async()
    {
        await Should_be_able_to_handle_idempotence_message_async(false);
    }

    private async Task Should_be_able_to_handle_idempotence_message_async(bool sync)
    {
        var idempotenceService = new Mock<IIdempotenceService>();
        var transportMessage = new TransportMessage();

        var observer = new IdempotenceObserver(idempotenceService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnIdempotenceMessageHandled>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Ignore));

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);

        if (sync)
        {
            pipeline.Execute();

            idempotenceService.Verify(m => m.MessageHandled(transportMessage));
        }
        else
        {
            await pipeline.ExecuteAsync();

            idempotenceService.Verify(m => m.MessageHandledAsync(transportMessage));
        }

        idempotenceService.VerifyNoOtherCalls();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Active));
    }
}