using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class GetWorkMessageObserverFixture
{
    [Test]
    public void Should_throw_exception_when_required_state_is_missing()
    {
        Should_throw_exception_when_required_state_is_missing_async(true);
    }

    [Test]
    public void Should_throw_exception_when_required_state_is_missing_async()
    {
        Should_throw_exception_when_required_state_is_missing_async(false);
    }

    private void Should_throw_exception_when_required_state_is_missing_async(bool sync)
    {
        var observer = new GetWorkMessageObserver();

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnGetMessage>();

        Core.Pipelines.PipelineException exception;

        if (sync)
        {
            exception = Assert.Throws<Core.Pipelines.PipelineException>(() => pipeline.Execute());
        }
        else
        {
            exception = Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync());
        }

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.InnerException?.Message, Contains.Substring(StateKeys.WorkQueue));
    }

    [Test]
    public void Should_be_able_to_abort_pipeline_when_no_message_is_available()
    {
        Should_be_able_to_abort_pipeline_when_no_message_is_available_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_abort_pipeline_when_no_message_is_available_async()
    {
        await Should_be_able_to_abort_pipeline_when_no_message_is_available_async(false);
    }

    private async Task Should_be_able_to_abort_pipeline_when_no_message_is_available_async(bool sync)
    {
        var workQueue = new Mock<IQueue>();
        var observer = new GetWorkMessageObserver();

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnGetMessage>();

        workQueue.Setup(m => m.GetMessage()).Returns((ReceivedMessage)null);
        workQueue.Setup(m => m.GetMessageAsync()).Returns(Task.FromResult((ReceivedMessage)null));

        pipeline.State.SetWorkQueue(workQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            workQueue.Verify(m => m.GetMessage(), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            workQueue.Verify(m => m.GetMessageAsync(), Times.Once);
        }

        Assert.That(pipeline.Aborted, Is.True);
        Assert.That(pipeline.State.GetWorking(), Is.False);

        workQueue.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_return_received_message()
    {
        Should_be_able_to_return_received_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_return_received_message_async()
    {
        await Should_be_able_to_return_received_message_async(false);
    }

    private async Task Should_be_able_to_return_received_message_async(bool sync)
    {
        var workQueue = new Mock<IQueue>();
        var observer = new GetWorkMessageObserver();

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnGetMessage>();

        var receivedMessage = new ReceivedMessage(Stream.Null, Guid.NewGuid());

        workQueue.Setup(m => m.GetMessage()).Returns(receivedMessage);
        workQueue.Setup(m => m.GetMessageAsync()).Returns(Task.FromResult(receivedMessage));

        pipeline.State.SetWorkQueue(workQueue.Object);

        if (sync)
        {
            pipeline.Execute();

            workQueue.Verify(m => m.GetMessage(), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            workQueue.Verify(m => m.GetMessageAsync(), Times.Once);
        }

        Assert.That(pipeline.Aborted, Is.False);
        Assert.That(pipeline.State.GetWorking(), Is.True);
        Assert.That(pipeline.State.GetReceivedMessage(), Is.SameAs(receivedMessage));

        workQueue.VerifyNoOtherCalls();
    }
}