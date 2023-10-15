using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DistributorMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_distribute_message()
    {
        Should_be_able_to_distribute_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_distribute_message_async()
    {
        await Should_be_able_to_distribute_message_async(false);
    }

    private async Task Should_be_able_to_distribute_message_async(bool sync)
    {
        var observer = new DistributorMessageObserver(new Mock<IWorkerAvailabilityService>().Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleDistributeMessage>();

        var transportMessage = new TransportMessage
        {
            RecipientInboxWorkQueueUri = "to-be-assigned"
        };

        var workerThreadAvailableCommand = new WorkerThreadAvailableCommand
        {
            InboxWorkQueueUri = "queue://the-worker"
        };

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetTransportMessageReceived(new TransportMessage());
        pipeline.State.SetAvailableWorker(new AvailableWorker(workerThreadAvailableCommand));

        if (sync)
        {
            pipeline.Execute(CancellationToken.None);
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }

        Assert.That(transportMessage.RecipientInboxWorkQueueUri, Is.EqualTo(workerThreadAvailableCommand.InboxWorkQueueUri));
        Assert.That(pipeline.State.GetTransportMessageReceived(), Is.Null);
    }

    [Test]
    public void Should_be_able_to_return_available_worker_on_pipeline_abort()
    {
        Should_be_able_to_return_available_worker_on_pipeline_abort_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_return_available_worker_on_pipeline_abort_async()
    {
        await Should_be_able_to_return_available_worker_on_pipeline_abort_async(false);
    }

    private async Task Should_be_able_to_return_available_worker_on_pipeline_abort_async(bool sync)
    {
        var workerAvailabilityService = new Mock<IWorkerAvailabilityService>();

        var observer = new DistributorMessageObserver(workerAvailabilityService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAbortPipeline>();

        var availableWorker = new AvailableWorker(new WorkerThreadAvailableCommand
        {
            InboxWorkQueueUri = "queue://the-worker"
        });

        pipeline.State.SetAvailableWorker(availableWorker);

        if (sync)
        {
            pipeline.Execute(CancellationToken.None);
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }

        workerAvailabilityService.Verify(m=> m.ReturnAvailableWorker(availableWorker));
        workerAvailabilityService.VerifyNoOtherCalls();
    }
}