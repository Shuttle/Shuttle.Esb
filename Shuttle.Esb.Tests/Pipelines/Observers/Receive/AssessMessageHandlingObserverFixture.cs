using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class AssessMessageHandlingObserverFixture
{
    [Test]
    public void Should_be_able_to_assess_message_handling()
    {
        Should_be_able_to_assess_message_handling_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_assess_message_handling_async()
    {
        await Should_be_able_to_assess_message_handling_async(false);
    }

    private async Task Should_be_able_to_assess_message_handling_async(bool sync)
    {
        var messageHandlingAssessor = new Mock<IMessageHandlingAssessor>();

        messageHandlingAssessor.SetupSequence(m=> m.IsSatisfiedBy(It.IsAny<OnAssessMessageHandling>()))
            .Returns(true)
            .Returns(false);

        var observer = new AssessMessageHandlingObserver(messageHandlingAssessor.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnAssessMessageHandling>();

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Active));

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Ignore));
    }
}