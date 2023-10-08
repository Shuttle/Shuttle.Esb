using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Esb.Tests.Pipelines.Observers;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class AcknowledgeMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_ignore_acknowledgement_on_failures()
    {
        Should_be_able_to_ignore_acknowledgement_on_failures_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_ignore_acknowledgement_on_failures_async()
    {
        await Should_be_able_to_ignore_acknowledgement_on_failures_async(false);
    }

    private async Task Should_be_able_to_ignore_acknowledgement_on_failures_async(bool sync)
    {
        var observer = new AcknowledgeMessageObserver();

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(new HandleExceptionObserver())
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>()
            .WithEvent<OnAcknowledgeMessage>();

        var workQueue = new Mock<IQueue>();

        pipeline.State.SetWorkQueue(workQueue.Object);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        workQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_acknowledge_message()
    {
        Should_be_able_to_acknowledge_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_acknowledge_message_async()
    {
        await Should_be_able_to_acknowledge_message_async(false);
    }

    private async Task Should_be_able_to_acknowledge_message_async(bool sync)
    {
        var observer = new AcknowledgeMessageObserver();

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAcknowledgeMessage>();

        var workQueue = new Mock<IQueue>();
        var receivedMessage = new ReceivedMessage(new Mock<Stream>().Object, Guid.NewGuid());

        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetReceivedMessage(receivedMessage);

        if (sync)
        {
            pipeline.Execute();

            workQueue.Verify(m => m.Acknowledge(receivedMessage.AcknowledgementToken));
        }
        else
        {
            await pipeline.ExecuteAsync();

            workQueue.Verify(m => m.AcknowledgeAsync(receivedMessage.AcknowledgementToken));
        }

        workQueue.VerifyNoOtherCalls();
    }
}