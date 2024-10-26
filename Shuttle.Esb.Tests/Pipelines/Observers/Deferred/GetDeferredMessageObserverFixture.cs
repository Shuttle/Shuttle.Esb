using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class GetDeferredMessageObserverFixture
{
    [Test]
    public async Task Should_be_able_to_get_a_message_from_the_deferred_queue_when_available_async()
    {
        var observer = new GetDeferredMessageObserver();

        var pipeline = new Pipeline().RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnGetMessage>();

        var deferredQueue = new Mock<IQueue>();
        var receivedMessage = new ReceivedMessage(new MemoryStream(), Guid.NewGuid());

        deferredQueue.SetupSequence(m => m.GetMessageAsync())
            .Returns(Task.FromResult<ReceivedMessage?>(receivedMessage))
            .Returns(Task.FromResult<ReceivedMessage?>(null));

        pipeline.State.SetDeferredQueue(deferredQueue.Object);

        await pipeline.ExecuteAsync();

        Assert.That(pipeline.State.GetReceivedMessage(), Is.Not.Null);
        Assert.That(pipeline.State.GetWorking(), Is.True);
        Assert.That(pipeline.Aborted, Is.False);

        pipeline.State.Clear();
        pipeline.State.SetDeferredQueue(deferredQueue.Object);

        await pipeline.ExecuteAsync();

        Assert.That(pipeline.State.GetReceivedMessage(), Is.Null);
        Assert.That(pipeline.State.GetWorking(), Is.False);
        Assert.That(pipeline.Aborted, Is.True);
    }
}