using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class MessageHandlingSpecificationObserverFixture
{
    [Test]
    public void Should_be_able_to_evaluate_message_handling_specification()
    {
        Should_be_able_to_evaluate_message_handling_specification_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_evaluate_message_handling_specification_async()
    {
        await Should_be_able_to_evaluate_message_handling_specification_async(false);
    }

    private async Task Should_be_able_to_evaluate_message_handling_specification_async(bool sync)
    {
        var messageHandlingSpecification = new Mock<IMessageHandlingSpecification>();

        messageHandlingSpecification.SetupSequence(m=> m.IsSatisfiedBy(It.IsAny<OnEvaluateMessageHandling>()))
            .Returns(true)
            .Returns(false);

        var observer = new MessageHandlingSpecificationObserver(messageHandlingSpecification.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnEvaluateMessageHandling>();

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