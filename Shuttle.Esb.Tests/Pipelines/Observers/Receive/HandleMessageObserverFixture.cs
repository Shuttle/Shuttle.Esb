using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class HandleMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_return_when_no_message_handling_is_required()
    {
        Should_be_able_to_return_when_no_message_handling_is_required_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_return_when_no_message_handling_is_required_async()
    {
        await Should_be_able_to_return_when_no_message_handling_is_required_async(false);
    }

    private async Task Should_be_able_to_return_when_no_message_handling_is_required_async(bool sync)
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();

        var observer = new HandleMessageObserver(Microsoft.Extensions.Options.Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleMessage>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        pipeline.State.SetProcessingStatus(ProcessingStatus.MessageHandled);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(new TransportMessage { ExpiryDate = DateTime.Now.AddDays(-1) });

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_handle_successful_invoke()
    {
        Should_be_able_to_handle_successful_invoke_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_handle_successful_invoke_async()
    {
        await Should_be_able_to_handle_successful_invoke_async(false);
    }

    private async Task Should_be_able_to_handle_successful_invoke_async(bool sync)
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();

        var observer = new HandleMessageObserver(Microsoft.Extensions.Options.Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);

        if (sync)
        {
            messageHandlerInvoker.Setup(m => m.Invoke(It.IsAny<IPipelineEvent>())).Returns(MessageHandlerInvokeResult.InvokedHandler("assembly-qualified-name"));

            pipeline.Execute();

            messageHandlerInvoker.Verify(m => m.Invoke(It.IsAny<IPipelineEvent>()));
        }
        else
        {
            messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineEvent>())).Returns(Task.FromResult(MessageHandlerInvokeResult.InvokedHandler("assembly-qualified-name")));

            await pipeline.ExecuteAsync();

            messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineEvent>()));
        }

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult().Invoked, Is.True);

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }
}