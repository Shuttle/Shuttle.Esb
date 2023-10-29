using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class OutboxExceptionObserverFixture
{
    [Test]
    public void Should_be_able_to_skip_when_exception_has_been_handled()
    {
        Should_be_able_to_skip_when_exception_has_been_handled_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_skip_when_exception_has_been_handled_async()
    {
        await Should_be_able_to_skip_when_exception_has_been_handled_async(false);
    }

    private async Task Should_be_able_to_skip_when_exception_has_been_handled_async(bool sync)
    {
        var serviceBusPolicy = new Mock<IServiceBusPolicy>();
        var serializer = new Mock<ISerializer>();
        var workQueue = new Mock<IQueue>();
        var errorQueue = new Mock<IQueue>();

        var observer = new OutboxExceptionObserver(serviceBusPolicy.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(new ThrowExceptionObserver())
            .RegisterObserver(new HandleExceptionObserver()) // marks exception as handled
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnException>();

        pipeline.State.SetWorkQueue(workQueue.Object);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.Aborted, Is.True);

        serviceBusPolicy.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
        workQueue.VerifyNoOtherCalls();
        errorQueue.VerifyNoOtherCalls();
    }
}