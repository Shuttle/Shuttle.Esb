using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class MessageHandlingSpecificationObserverFixture
{
    [Test]
    public async Task Should_be_able_to_evaluate_message_handling_specification_async()
    {
        var messageHandlingSpecification = new Mock<IMessageHandlingSpecification>();

        messageHandlingSpecification.SetupSequence(m => m.IsSatisfiedBy(It.IsAny<IPipelineContext>()))
            .Returns(true)
            .Returns(false);

        var observer = new MessageHandlingSpecificationObserver(messageHandlingSpecification.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnEvaluateMessageHandling>();

        await pipeline.ExecuteAsync();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Active));

        await pipeline.ExecuteAsync();

        Assert.That(pipeline.State.GetProcessingStatus(), Is.EqualTo(ProcessingStatus.Ignore));
    }
}